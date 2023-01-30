using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

using Icarus.Misc;

namespace Icarus.Orbit {
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(UpdateOrbitSystemGroup))]
    public partial class UpdatePlayerRelationTags : SystemBase {
        private EntityQuery CurrentSiblings;
        private EntityQuery DesiredSiblings;
        private ComponentLookup<PlanetTag> PlanetTags;

        protected override void OnCreate() {
            PlanetTags = GetComponentLookup<PlanetTag>(true);
            DesiredSiblings = new EntityQueryBuilder(Allocator.TempJob)
                .WithNone<PlayerOrbitTag>()
                .WithAll<OrbitalParent>()
                .Build(this);
            CurrentSiblings = new EntityQueryBuilder(Allocator.TempJob)
                .WithAll<PlayerSiblingOrbitTag>()
                .Build(this);
        }
        
        protected override void OnUpdate() {
            PlanetTags.Update(this);
            var ecb0 = new EntityCommandBuffer(Allocator.TempJob);
            var ecb1 = new EntityCommandBuffer(Allocator.TempJob);

            // check parent
            var player = GetSingletonEntity<PlayerOrbitTag>();
            var parent = this.EntityManager.GetSharedComponent<OrbitalParent>(player);
            Entity cparent;
            var found = SystemAPI.TryGetSingletonEntity<PlayerParentOrbitTag>(out cparent);
            if (parent.Value != cparent) {
                if (found) {
                    ecb0.RemoveComponent<PlayerParentOrbitTag>(cparent);
                    // disable rendering only if a non-planet
                    if (!PlanetTags.HasComponent(cparent)) {
                        ecb0.RemoveComponent<OrbitRenderingEnabled>(cparent);
                        ecb0.AddComponent<OrbitRenderingDisabled>(cparent);
                    }
                }
                ecb0.AddComponent<PlayerParentOrbitTag>(parent.Value);
                ecb0.RemoveComponent<OrbitRenderingDisabled>(parent.Value);
                ecb0.AddComponent<OrbitRenderingEnabled>(parent.Value);
            }

            // update desired query
            DesiredSiblings.SetSharedComponentFilter(parent);

            // add new siblings
            var job0 = new UpdateSiblingRelationJob {
                pecb = ecb0.AsParallelWriter(),
                OtherQuery = DesiredSiblings,
                PlanetTags = PlanetTags,
                add = false
            }.Schedule(CurrentSiblings, this.Dependency);

            // remove old siblings
            var job1 = new UpdateSiblingRelationJob {
                pecb = ecb1.AsParallelWriter(),
                OtherQuery = CurrentSiblings,
                PlanetTags = PlanetTags,
                add = true
            }.Schedule(DesiredSiblings, this.Dependency);
            
            this.Dependency = JobHandle.CombineDependencies(job0, job1);
            this.Dependency.Complete();
            
            ecb0.Playback(this.EntityManager);
            ecb0.Dispose();
            ecb1.Playback(this.EntityManager);
            ecb1.Dispose();
        }
    }

    [WithAll(typeof(NeverMatchTag))] // disable internal query generation
    public partial struct UpdateSiblingRelationJob : IJobEntity {
        public EntityCommandBuffer.ParallelWriter pecb;
        [ReadOnly]
        [Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
        public EntityQuery OtherQuery;
        [ReadOnly]
        public ComponentLookup<PlanetTag> PlanetTags;
        [ReadOnly]
        public bool add;
        
        public void Execute(Entity entity, [ChunkIndexInQuery] int index) {
            if (!OtherQuery.Matches(entity)) {
                if (add) {
                    pecb.AddComponent<PlayerSiblingOrbitTag>(index, entity);
                    pecb.RemoveComponent<OrbitRenderingDisabled>(index, entity);
                    pecb.AddComponent<OrbitRenderingEnabled>(index, entity);
                } else {
                    pecb.RemoveComponent<PlayerSiblingOrbitTag>(index, entity);
                    // disable rendering for non-planet, non-siblings
                    if (!PlanetTags.HasComponent(entity)) {
                        pecb.RemoveComponent<OrbitRenderingEnabled>(index, entity);
                        pecb.AddComponent<OrbitRenderingDisabled>(index, entity);
                    }
                }
            }
        }
    }
}
