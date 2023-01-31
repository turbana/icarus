using System;

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

using Icarus.Misc;

namespace Icarus.Orbit {
    [UpdateInGroup(typeof(UpdateOrbitSystemGroup))]
    [UpdateAfter(typeof(UpdateOrbitalPositionSystem))]
    [UpdateAfter(typeof(UpdateOrbitalRenderingSystem))]
    [UpdateAfter(typeof(UpdatePlayerRelationTags))]
    public partial class UpdateGamePositionSystem : SystemBase {
        private EntityQuery SiblingQuery;
        private EntityQuery PlanetQuery;
        private EntityQuery ParentQuery;

        [BurstCompile]
        protected override void OnCreate() {
            SiblingQuery = new EntityQueryBuilder(Allocator.TempJob)
                .WithAll<PlayerSiblingOrbitTag, OrbitRenderingEnabled>()
                .WithNone<PlanetTag>()
                .WithAllRW<LocalTransform>()
                .WithAll<OrbitalPosition, OrbitalParameters, OrbitalScale>()
                .WithAll<RotationalParameters, OrbitalParent>()
                .Build(this);
            PlanetQuery = new EntityQueryBuilder(Allocator.TempJob)
                .WithAll<PlanetTag, OrbitRenderingEnabled>()
                .WithNone<PlayerParentOrbitTag>()
                .WithAllRW<LocalTransform>()
                .WithAll<OrbitalPosition, OrbitalParameters, OrbitalScale>()
                .WithAll<RotationalParameters, OrbitalParent>()
                .Build(this);
            ParentQuery = new EntityQueryBuilder(Allocator.TempJob)
                .WithAll<PlayerParentOrbitTag, OrbitRenderingEnabled>()
                .WithAllRW<LocalTransform>()
                .WithAll<OrbitalPosition, OrbitalParameters, OrbitalScale>()
                .WithAll<RotationalParameters, OrbitalParent>()
                .Build(this);
        }

        [BurstCompile]
        protected override void OnUpdate() {
            Entity sun = GetSingletonEntity<SunTag>();
            Entity player = GetSingletonEntity<PlayerOrbitTag>();
            OrbitalParent playerParent = this.EntityManager.GetSharedComponent<OrbitalParent>(player);
            OrbitalPosition playerPosition = GetComponent<OrbitalPosition>(player);
            PlayerRotation playerRotation = GetComponent<PlayerRotation>(
                GetSingletonEntity<PlayerTag>());
            
            // planets
            var handle0 = new UpdateGamePositionJob {
                player = player,
                playerParent = playerParent,
                playerPosition = playerPosition,
                playerRotation = playerRotation,
                isSibling = false,
                isParent = false
            }.ScheduleParallel(PlanetQuery, this.Dependency);
            
            // siblings
            var handle1 = new UpdateGamePositionJob {
                player = player,
                playerParent = playerParent,
                playerPosition = playerPosition,
                playerRotation = playerRotation,
                isSibling = true,
                isParent = false
            }.ScheduleParallel(SiblingQuery, handle0);

            // parent
            var handle2 = new UpdateGamePositionJob {
                player = player,
                playerParent = playerParent,
                playerPosition = playerPosition,
                playerRotation = playerRotation,
                isSibling = false,
                isParent = true
            }.ScheduleParallel(ParentQuery, handle1);

            this.Dependency = handle2;
        }
    }

    [WithAll(typeof(NeverMatchTag))]
    [BurstCompile]
    public partial struct UpdateGamePositionJob : IJobEntity {
        public EntityCommandBuffer.ParallelWriter ecb;
        public Entity player;
        public OrbitalParent playerParent;
        public OrbitalPosition playerPosition;
        public PlayerRotation playerRotation;
        public bool isSibling;
        public bool isParent;
        
        [BurstCompile]
        void Execute(Entity entity,
                     ref LocalTransform transform,
                     in OrbitalPosition pos, in OrbitalParameters parms, in OrbitalScale scale,
                     in OrbitalParentPosition parentPos, in RotationalParameters rot)
        {
            float rscale;
            float dist;
            float3 ppos;
            float3 newpos;
            float3 playerPos;

            if (isSibling) {
                ppos = pos.LocalToParent.Position;
                playerPos = playerPosition.LocalToParent.Position;
            } else if (isParent) {
                ppos = float3.zero;
                playerPos = playerPosition.LocalToParent.Position;
            } else {
                ppos = pos.LocalToWorld.Position;
                playerPos = playerPosition.LocalToWorld.Position;
            }
            
            ppos = ppos - playerPos;
            dist = math.length(ppos);
            
            if (dist < 1f) {
                newpos = ppos * 1000f;
                rscale = 1f;
            } else {
                float sdist = dist - scale.Radius;
                float X = 149597870.700f;
                float desired = 1000f + math.sqrt(sdist / X) * 100f;
                float theta = scale.Radius / dist;
                float radius = -((theta * desired) / (theta - 1f));
                newpos = math.normalize(ppos) * (desired + radius);
                rscale = radius * 2f;
            }
                        
            quaternion prot = math.inverse(playerRotation.Value);
            quaternion pprot = playerRotation.Value;
            quaternion orot = math.mul(rot.AxialTilt, rot.AxialRotation);
            newpos = math.mul(prot, newpos);
            orot = math.mul(prot, orot);

            transform = LocalTransform
                .FromPositionRotationScale(newpos, orot, rscale);
        }
    }
}
