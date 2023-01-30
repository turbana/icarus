using System;

using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

using Icarus.Misc;

namespace Icarus.Orbit {
    [UpdateInGroup(typeof(UpdateOrbitSystemGroup))]
    [UpdateAfter(typeof(UpdateOrbitalPositionSystem))]
    [UpdateAfter(typeof(UpdateOrbitalRenderingSystem))]
    public partial class UpdateGamePositionSystem : SystemBase {
        private EntityQuery SiblingQuery;
        private EntityQuery PlanetQuery;
        private EntityQuery ParentQuery;

        protected override void OnCreate() {
            SiblingQuery = new EntityQueryBuilder(Allocator.TempJob)
                .WithNone<PlayerOrbitTag>()
                .WithAllRW<LocalTransform>()
                .WithAll<OrbitalPosition, OrbitalParameters, OrbitalScale>()
                .WithAll<RotationalParameters, OrbitalParent>()
                .Build(this);
            PlanetQuery = new EntityQueryBuilder(Allocator.TempJob)
                .WithAll<PlanetTag>()
                .WithNone<PlayerParentOrbitTag>()
                .WithAllRW<LocalTransform>()
                .WithAll<OrbitalPosition, OrbitalParameters, OrbitalScale>()
                .WithAll<RotationalParameters, OrbitalParent>()
                .Build(this);
            ParentQuery = new EntityQueryBuilder(Allocator.TempJob)
                .WithAll<PlayerParentOrbitTag>()
                .WithAllRW<LocalTransform>()
                .WithAll<OrbitalPosition, OrbitalParameters, OrbitalScale>()
                .WithAll<RotationalParameters, OrbitalParent>()
                .Build(this);
        }
        
        protected override void OnUpdate() {
            Entity sun = GetSingletonEntity<SunTag>();
            Entity player = GetSingletonEntity<PlayerOrbitTag>();
            OrbitalParent playerParent = this.EntityManager.GetSharedComponent<OrbitalParent>(player);
            OrbitalPosition playerPosition = GetComponent<OrbitalPosition>(player);
            PlayerRotation playerRotation = GetComponent<PlayerRotation>(
                GetSingletonEntity<PlayerTag>());
            
            // planets
            PlanetQuery.SetSharedComponentFilter(new OrbitalParent { Value = sun });
            var handle0 = new UpdateGamePositionJob {
                player = player,
                playerParent = playerParent,
                playerPosition = playerPosition,
                playerRotation = playerRotation,
                isSibling = false,
                isParent = false
            }.ScheduleParallel(PlanetQuery, this.Dependency);
            
            // siblings
            SiblingQuery.SetSharedComponentFilter(playerParent);
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
    public partial struct UpdateGamePositionJob : IJobEntity {
        public EntityCommandBuffer.ParallelWriter ecb;
        public Entity player;
        public OrbitalParent playerParent;
        public OrbitalPosition playerPosition;
        public PlayerRotation playerRotation;
        public bool isSibling;
        public bool isParent;
        
        void Execute(Entity entity,
                     ref LocalTransform transform,
                     in OrbitalPosition pos, in OrbitalParameters parms, in OrbitalScale scale,
                     in OrbitalParentPosition parentPos, in RotationalParameters rot)
        {
            float rscale;
            float X = 149597870.700f;
            float3 ppos;
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
                        
            float dist = math.distance(ppos, playerPos);
            float sdist = dist - scale.Radius;
            float desired = 1000f + math.sqrt(sdist / X) * 100f;
            float A = scale.Radius / dist;
            float S = -((A * desired) / (A - 1f));
            rscale = S * 2f;
            ppos = ppos - playerPos;
            float3 newpos = math.normalize(ppos) * (desired + rscale / 2f);
                        
            if (dist < 1000f) {
                newpos = ppos;
                rscale = 1f;
            }
                        
            quaternion prot = math.inverse(playerRotation.Value);
            quaternion pprot = playerRotation.Value;
            quaternion orot = math.mul(rot.AxialTilt, rot.AxialRotation);
            newpos = math.mul(prot, newpos);
            orot = math.mul(prot, orot);

            transform = LocalTransform
                .FromPositionRotationScale(newpos, orot, rscale);
                        
            // float wdist = math.length(newpos);
            // float wr = rscale / 2f;
            // float wmag = 2f * math.degrees(math.asin(wr / wdist));
            // float gdist = math.distance(newpos, float3.zero);
            // float rdist = math.distance(pos.LocalToWorld.Position, playerPosition.LocalToWorld.Position);
            // float rr = scale.Radius;
            // float rmag = 2f * math.degrees(math.asin(rr / rdist));
            // if (scale.Radius == 6371f) {
            //     UnityEngine.Debug.Log($"i={entityInQueryIndex} wmag={wmag} rmag={rmag} dist={dist} rdist={rdist} gdist={gdist} rr={rr} <<<R={scale.Radius}>>> pos={newpos}");
            // }
        }
    }
}
