using System;

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

using Icarus.Graphics;
using Icarus.Mathematics;
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
            Entity sun = SystemAPI.GetSingletonEntity<SunTag>();
            Entity player = SystemAPI.GetSingletonEntity<PlayerOrbitTag>();
            OrbitalParent playerParent = this.EntityManager.GetSharedComponent<OrbitalParent>(player);
            OrbitalPosition playerPosition = SystemAPI.GetComponent<OrbitalPosition>(player);
            PlayerRotation playerRotation = SystemAPI.GetComponent<PlayerRotation>(
                SystemAPI.GetSingletonEntity<PlayerTag>());
            
            TextUpdateSystem.Update("Player.ElapsedTime", playerPosition.ElapsedTime.ToString("000000000.00"));
            
            // planets
            var handle0 = new UpdateGamePositionJob {
                player = player,
                playerParent = playerParent,
                playerPosition = playerPosition,
                playerRotation = playerRotation,
                isSibling = false,
                isParent = false,
                distance = 20000.0
            }.ScheduleParallel(PlanetQuery, this.Dependency);
            
            // siblings
            var handle1 = new UpdateGamePositionJob {
                player = player,
                playerParent = playerParent,
                playerPosition = playerPosition,
                playerRotation = playerRotation,
                isSibling = true,
                isParent = false,
                distance = 20000.0
            }.ScheduleParallel(SiblingQuery, handle0);

            // parent
            var handle2 = new UpdateGamePositionJob {
                player = player,
                playerParent = playerParent,
                playerPosition = playerPosition,
                playerRotation = playerRotation,
                isSibling = false,
                isParent = true,
                distance = 1000.0
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
        public double distance;
        
        [BurstCompile]
        void Execute(Entity entity,
                     ref LocalTransform transform,
                     in OrbitalPosition pos, in OrbitalParameters parms, in OrbitalScale scale,
                     in OrbitalParentPosition parentPos, in RotationalParameters rot)
        {
            double rscale;
            double dist;
            double3 ppos;
            double3 newpos;
            double3 playerPos;

            if (isSibling) {
                ppos = pos.LocalToParent;
                playerPos = playerPosition.LocalToParent;
            } else if (isParent) {
                ppos = double3.zero;
                playerPos = playerPosition.LocalToParent;
            } else {
                ppos = pos.LocalToWorld;
                playerPos = playerPosition.LocalToWorld;
            }
            
            ppos = ppos - playerPos;
            dist = math.length(ppos);

            if (dist < 1.0) {
                newpos = ppos * distance;
                rscale = 1.0;
            } else {
                double sdist = dist - scale.Radius;
                double X = 149597870.700;
                double increment = distance / 100;
                double desired = distance + math.sqrt(sdist / X) * increment;
                double theta = scale.Radius / dist;
                double radius = -((theta * desired) / (theta - 1.0));
                newpos = math.normalize(ppos) * (desired + radius);
                rscale = radius * 2.0;
            }

            dquaternion prot = dmath.inverse(playerRotation.Value);
            dquaternion pprot = playerRotation.Value;
            dquaternion orot = dmath.mul(rot.AxialTilt, rot.AxialRotation);
            newpos = dmath.mul(prot, newpos);
            orot = dmath.mul(prot, orot);

            transform = LocalTransform
                .FromPositionRotationScale((float3)newpos, orot, (float)rscale);
        }
    }
}
