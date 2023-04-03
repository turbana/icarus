using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;

namespace Icarus.UI {
    [BurstCompile]
    [UpdateInGroup(typeof(IcarusPresentationSystemGroup), OrderLast=true)]
    public partial class DatumCleanupSystem : SystemBase {
        [BurstCompile]
        protected override void OnUpdate() {
            RefRW<DatumCollection> datums;
            if (!SystemAPI.TryGetSingletonRW<DatumCollection>(out datums)) {
                // DatumCollection is in a subscene that might not be loaded yet.
                return;
            }
            datums.ValueRW.ResetDirty();
        }
    }
}
