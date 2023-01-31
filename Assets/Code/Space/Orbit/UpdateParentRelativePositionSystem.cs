using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Icarus.Orbit {
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(UpdateOrbitSystemGroup))]
    [UpdateBefore(typeof(UpdateOrbitalPositionSystem))]
    [BurstCompile]
    public partial class UpdateParentRelativePositionSystem : SystemBase {
        [BurstCompile]
        protected override void OnUpdate() {
            var OrbitalParentTypeHandle = GetSharedComponentTypeHandle<OrbitalParent>();
            var OrbitalPositionLookup = GetComponentLookup<OrbitalPosition>(true);
            new UpdateParentRelativePositionJob {
                OrbitalParentTypeHandle = OrbitalParentTypeHandle,
                OrbitalPositionLookup = OrbitalPositionLookup
            }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct UpdateParentRelativePositionJob : IJobEntity, IJobEntityChunkBeginEnd {
        [ReadOnly]
        public SharedComponentTypeHandle<OrbitalParent> OrbitalParentTypeHandle;
        [ReadOnly]
        public ComponentLookup<OrbitalPosition> OrbitalPositionLookup;
        
        private LocalTransform shared;

        [BurstCompile]
        public bool OnChunkBegin(in ArchetypeChunk chunk, int index, bool useMask, in v128 mask) {
            var parent = chunk.GetSharedComponent<OrbitalParent>(OrbitalParentTypeHandle);
            shared = OrbitalPositionLookup[parent.Value].LocalToWorld;
            return true;
        }

        [BurstCompile]
        public void OnChunkEnd(in ArchetypeChunk chunk, int index, bool useMask, in v128 mask, bool wasExecuted) {}

        [BurstCompile]
        public void Execute(Entity entity, ref OrbitalParentPosition ppos) {
            ppos.Value = shared;
        }
    }
}
