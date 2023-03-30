using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;

namespace Icarus.UI {
    [BurstCompile]
    [UpdateInGroup(typeof(IcarusPresentationSystemGroup), OrderLast=true)]
    public partial class DatumCleanupSystem : SystemBase {
        [BurstCompile]
        protected override void OnUpdate() {
            // this.Dependency.Complete();
            var datums = SystemAPI.GetSingletonRW<DatumCollection>();
            // // datums.Dirty.Clear();
            datums.ValueRW.ResetDirty();

            // Entities
            //     .ForEach((ref DatumCollection datums) => {
            //         datums.ResetDirty();
            //     })
            //     .Run();
        }
    }
}
