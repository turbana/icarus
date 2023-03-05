using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Icarus.UI {
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(IcarusInteractionSystemGroup))]
    public partial class DatumUpdateSystem : SystemBase {
        public ComponentLookup<LocalTransform> LocalTransformLookup;

        [BurstCompile]
        protected override void OnCreate() {
            LocalTransformLookup = GetComponentLookup<LocalTransform>(false);
        }
        
        [BurstCompile]
        protected override void OnUpdate() {
            LocalTransformLookup.Update(this);
            var LTL = LocalTransformLookup;
            var registry = SystemAPI.GetSingleton<DatumRegistry>();

            Entities
                .WithChangeFilter<DatumByte>()
                .ForEach((Entity entity, in DatumByte datum, in DynamicBuffer<DatumBackRef> buffer) => {
                        for (int i=0; i<buffer.Length; i++) {
                            var control = SystemAPI.GetAspectRO<ControlAspect>(buffer[i].Value);
                            var transform = control.GetLocalTransform(in datum);
                            LTL[control.Root] = transform;
                        }
                    })
                .Schedule();
        }
    }
}
