using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Icarus.Orbit {
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(UpdateOrbitSystemGroup))]
    [UpdateBefore(typeof(UpdateOrbitalPositionSystem))]
    public partial class UpdateParentRelativePositionSystem : SystemBase {
        protected override void OnUpdate() {
            var OrbitalParentTypeHandle = GetSharedComponentTypeHandle<OrbitalParent>();
            var OrbitalPositionLookup = GetComponentLookup<OrbitalPosition>(true);
            new UpdateParentRelativePositionJob {
                OrbitalParentTypeHandle = OrbitalParentTypeHandle,
                OrbitalPositionLookup = OrbitalPositionLookup
            }.ScheduleParallel();
        }
    }

    public partial struct UpdateParentRelativePositionJob : IJobEntity, IJobEntityChunkBeginEnd {
        [ReadOnly]
        public SharedComponentTypeHandle<OrbitalParent> OrbitalParentTypeHandle;
        [ReadOnly]
        public ComponentLookup<OrbitalPosition> OrbitalPositionLookup;
        
        private LocalTransform shared;

        public bool OnChunkBegin(in ArchetypeChunk chunk, int index, bool useMask, in v128 mask) {
            var parent = chunk.GetSharedComponent<OrbitalParent>(OrbitalParentTypeHandle);
            shared = OrbitalPositionLookup[parent.Value].LocalToWorld;
            return true;
        }

        public void OnChunkEnd(in ArchetypeChunk chunk, int index, bool useMask, in v128 mask, bool wasExecuted) {}

        public void Execute(Entity entity, ref OrbitalParentPosition ppos) {
            ppos.Value = shared;
        }
    }
}
