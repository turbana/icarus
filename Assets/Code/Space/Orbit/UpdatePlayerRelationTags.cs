using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

using Icarus.Misc;

namespace Icarus.Orbit {
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(UpdateOrbitSystemGroup))]
    [BurstCompile]
    public partial class UpdatePlayerRelationTags : SystemBase {
        private ComponentLookup<PlanetTag> PlanetTags;
        private ComponentLookup<PlayerSiblingOrbitTag> PlayerSiblings;
        private SharedComponentTypeHandle<OrbitalParent> OrbitalParentTypeHandle;

        [BurstCompile]
        protected override void OnCreate() {
            PlanetTags = GetComponentLookup<PlanetTag>(true);
            PlayerSiblings = GetComponentLookup<PlayerSiblingOrbitTag>(false);
            OrbitalParentTypeHandle = GetSharedComponentTypeHandle<OrbitalParent>();
        }
        
        [BurstCompile]
        protected override void OnUpdate() {
            PlanetTags.Update(this);
            PlayerSiblings.Update(this);
            OrbitalParentTypeHandle.Update(this);
            
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            var player = SystemAPI.GetSingletonEntity<PlayerOrbitTag>();
            var parent = this.EntityManager.GetSharedComponent<OrbitalParent>(player).Value;
            Entity cparent;
            var found = SystemAPI.TryGetSingletonEntity<PlayerParentOrbitTag>(out cparent);

            // update parent
            if (cparent != parent) {
                if (found) {
                    ecb.RemoveComponent<PlayerParentOrbitTag>(cparent);
                }
                ecb.AddComponent<PlayerParentOrbitTag>(parent);
            }

            // update siblings
            new UpdateSiblingRelationJob {
                pecb = ecb.AsParallelWriter(),
                OrbitalParentTypeHandle = OrbitalParentTypeHandle,
                PlayerParent = parent,
                PlanetTags = PlanetTags,
                PlayerSiblings = PlayerSiblings
            }.ScheduleParallel();

            this.Dependency.Complete();
            ecb.Playback(this.EntityManager);
            ecb.Dispose();
        }
    }

    [WithAll(typeof(OrbitalParent))]
    [WithNone(typeof(PlayerOrbitTag))]
    [WithChangeFilter(typeof(OrbitalParent))]
    [BurstCompile]
    public partial struct UpdateSiblingRelationJob : IJobEntity, IJobEntityChunkBeginEnd {
        public EntityCommandBuffer.ParallelWriter pecb;
        [ReadOnly]
        public SharedComponentTypeHandle<OrbitalParent> OrbitalParentTypeHandle;
        [ReadOnly]
        public Entity PlayerParent;
        [ReadOnly]
        public ComponentLookup<PlanetTag> PlanetTags;
        [ReadOnly]
        public ComponentLookup<PlayerSiblingOrbitTag> PlayerSiblings;

        private Entity parent;

        [BurstCompile]
        public bool OnChunkBegin(in ArchetypeChunk chunk, int index, bool useMask, in v128 mask) {
            var comp = chunk.GetSharedComponent<OrbitalParent>(OrbitalParentTypeHandle);
            parent = comp.Value;
            return true;
        }

        [BurstCompile]
        public void OnChunkEnd(in ArchetypeChunk chunk, int index, bool useMask, in v128 mask, bool wasExecuted) {}

        [BurstCompile]
        public void Execute(Entity entity, [EntityIndexInQuery] int index) {
            var planet = PlanetTags.HasComponent(entity);
            var sibling = PlayerSiblings.HasComponent(entity);
            
            if (parent == PlayerParent || planet) {
                // set sibling
                if (!sibling) {
                    pecb.AddComponent<PlayerSiblingOrbitTag>(index, entity);
                    pecb.RemoveComponent<OrbitRenderingDisabled>(index, entity);
                    pecb.AddComponent<OrbitRenderingEnabled>(index, entity);
                }
            } else {
                // clear sibling
                if (sibling) {
                    pecb.RemoveComponent<PlayerSiblingOrbitTag>(index, entity);
                    // disable rendering for non-planet, non-siblings
                    if (!planet) {
                        pecb.RemoveComponent<OrbitRenderingEnabled>(index, entity);
                        pecb.AddComponent<OrbitRenderingDisabled>(index, entity);
                    }
                }
            }
        }
    }
}
