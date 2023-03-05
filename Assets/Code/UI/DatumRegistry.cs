using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

using Icarus.Loading;

namespace Icarus.UI {
    /* InitializeDatumSystem will find all UninitializedDatumRefs, find the
     * correct Datum and replace it with a DatumRef. */
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(IcarusLoadingSystemGroup))]
    public partial class InitializeDatumSystem : SystemBase {
        [BurstCompile]
        protected override void OnUpdate() {
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            var registry = SystemAPI.GetSingleton<DatumRegistry>();

            Entities
                .ForEach((Entity entity, ref UninitializedDatumRef datum) => {
                    var dentity = registry.Lookup(ref ecb, in datum);
                    if (dentity == Entity.Null) {
                        throw new System.Exception($"error creating Datum from UnitializedDatumRef[{datum.ID}]");
                    }
                    ecb.RemoveComponent<UninitializedDatumRef>(entity);
                    ecb.AddComponent<DatumRef>(entity, new DatumRef {
                            Entity = dentity,
                            Type = datum.Type,
                        });
                    registry.AddBackRef(ref ecb, in datum.ID, in entity);
                    // UnityEngine.Debug.Log($"added datum ({datum.ID}) [{datum.Type}]");
                    })
                .Schedule();

            this.Dependency.Complete();
            ecb.Playback(this.EntityManager);
            ecb.Dispose();
        }
    }
}
