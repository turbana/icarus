using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;

namespace Icarus.UI {
    [BurstCompile]
    [UpdateInGroup(typeof(IcarusPresentationSystemGroup), OrderLast=true)]
    public partial class DatumCleanupSystem : SystemBase {
        [BurstCompile]
        protected override void OnUpdate() {
            var doubles = Entities
                .WithChangeFilter<DatumDouble>()
                .ForEach((ref DatumDouble datum) => { datum.PreviousValue = datum.Value; })
                .ScheduleParallel(this.Dependency);
            
            var strings = Entities
                .WithChangeFilter<DatumString64>()
                .ForEach((ref DatumString64 datum) => { datum.PreviousValue = datum.Value; })
                .ScheduleParallel(this.Dependency);
            
            this.Dependency = JobHandle.CombineDependencies(doubles, strings);
        }
    }
}
