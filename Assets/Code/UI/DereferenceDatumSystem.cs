using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;

using Icarus.Loading;

namespace Icarus.UI {
    /* DeferenceDatumSystem will find all UninitializedDatumRefs, find the
     * correct Datum and replace it with a DatumRef. */
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(IcarusLoadingSystemGroup))]
    public partial class DereferenceDatumSystem : SystemBase {
        private NativeParallelHashMap<FixedString64Bytes, Entity> Doubles;
        private NativeParallelHashMap<FixedString64Bytes, Entity> String64s;
        private NativeParallelHashMap<FixedString512Bytes, Entity> String512s;
        private EntityQuery DatumRefQuery;
        private EntityQuery DatumRefBufferQuery;

        [BurstCompile]
        protected override void OnCreate() {
            var N = 1000;
            this.Doubles = new NativeParallelHashMap<FixedString64Bytes, Entity>(N, Allocator.Persistent);
            this.String64s = new NativeParallelHashMap<FixedString64Bytes, Entity>(N, Allocator.Persistent);
            this.String512s = new NativeParallelHashMap<FixedString512Bytes, Entity>(N, Allocator.Persistent);
            RequireForUpdate(DatumRefQuery);
            RequireForUpdate(DatumRefBufferQuery);
        }
        
        [BurstCompile]
        protected override void OnUpdate() {
            var new_doubles = new NativeHashSet<FixedString64Bytes>(64, Allocator.TempJob);
            var new_string64s = new NativeHashSet<FixedString64Bytes>(64, Allocator.TempJob);
            var new_string512s = new NativeHashSet<FixedString64Bytes>(64, Allocator.TempJob);
            var doubles = this.Doubles;
            var string64s = this.String64s;
            var string512s = this.String512s;

            // load new DatumDoubles
            var job_doubles = Entities
                .WithChangeFilter<DatumDouble>()
                .ForEach((Entity entity, in DatumDouble datum) => {
                    doubles[datum.ID] = entity;
                })
                .Schedule(this.Dependency);

            // load new DatumString64s
            var job_string64s = Entities
                .WithChangeFilter<DatumString64>()
                .ForEach((Entity entity, in DatumString64 datum) => {
                    string64s[datum.ID] = entity;
                })
                .Schedule(this.Dependency);

            // load new DatumString512s
            var job_string512s = Entities
                .WithChangeFilter<DatumString512>()
                .ForEach((Entity entity, in DatumString512 datum) => {
                    string512s[datum.ID] = entity;
                })
                .Schedule(this.Dependency);

            // find UninitializedDatumRefs
            var job_find_datumrefs = Entities
                .WithStoreEntityQueryInField(ref DatumRefQuery)
                .ForEach((in UninitializedDatumRef datum) => {
                    switch (datum.Type) {
                        case DatumType.Double:
                            new_doubles.Add(datum.ID); break;
                        case DatumType.String64:
                            new_string64s.Add(datum.ID); break;
                        case DatumType.String512:
                            new_string512s.Add(datum.ID); break;
                        default:
                            throw new System.Exception($"unknown DatumType: {datum.Type}");
                    }
                })
                .Schedule(this.Dependency);
            
            // find UninitializedDatumRefBuffers
            var job_finds = Entities
                .WithStoreEntityQueryInField(ref DatumRefBufferQuery)
                .ForEach((in DynamicBuffer<UninitializedDatumRefBuffer> buffer) => {
                    for (int i=0; i<buffer.Length; i++) {
                        var datum = buffer[i];
                        switch (datum.Type) {
                            case DatumType.Double:
                                new_doubles.Add(datum.ID); break;
                            case DatumType.String64:
                                new_string64s.Add(datum.ID); break;
                            case DatumType.String512:
                                new_string512s.Add(datum.ID); break;
                            default:
                                throw new System.Exception($"unknown DatumType: {datum.Type}");
                        }
                    }
                })
                .Schedule(job_find_datumrefs);

            var job_strings = JobHandle.CombineDependencies(job_string64s, job_string512s);
            var job_loads = JobHandle.CombineDependencies(job_doubles, job_strings);
            this.Dependency = JobHandle.CombineDependencies(job_loads, job_finds);
            this.Dependency.Complete();

            // NOTE: create datums so that .Dirty is true on the first frame.

            // create new DatumDouble entities
            foreach (var ID in new_doubles) {
                var entity = EntityManager.CreateEntity();
                var datum = new DatumDouble {
                    ID = ID,
                    Value = 0.0,
                    PreviousValue = double.NegativeInfinity,
                };
                EntityManager.AddComponentData<DatumDouble>(entity, datum);
                doubles[ID] = entity;
            }

            // create new DatumString64 entities
            foreach (var ID in new_string64s) {
                var entity = EntityManager.CreateEntity();
                var datum = new DatumString64 {
                    ID = ID,
                    Value = "",
                    PreviousValue = " ",
                };
                EntityManager.AddComponentData<DatumString64>(entity, datum);
                string64s[ID] = entity;
            }

            // create new DatumString512 entities
            foreach (var ID in new_string512s) {
                var entity = EntityManager.CreateEntity();
                var datum = new DatumString512 {
                    ID = ID,
                    Value = "",
                    PreviousValue = " ",
                };
                EntityManager.AddComponentData<DatumString512>(entity, datum);
                string512s[ID] = entity;
            }

            // rest will use an ecb
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            
            // realize DatumRefs
            Entities
                .ForEach((Entity entity, in UninitializedDatumRef datum) => {
                    var dentity = datum.Type switch {
                        DatumType.Double => doubles[datum.ID],
                        DatumType.String64 => string64s[datum.ID],
                        DatumType.String512 => string512s[datum.ID],
                        _ => throw new System.Exception($"unknown DatumType: {datum.Type}"),

                    };
                    ecb.AddComponent<DatumRef>(entity, new DatumRef {
                            Entity = dentity,
                            Type = datum.Type,
                        });
                    ecb.RemoveComponent<UninitializedDatumRef>(entity);
                })
                .Schedule();

            // realize DatumRefBuffers
            Entities
                .ForEach((Entity entity, in DynamicBuffer<UninitializedDatumRefBuffer> buffer) => {
                    var nbuffer = ecb.AddBuffer<DatumRefBuffer>(entity);
                    for (int i=0; i<buffer.Length; i++) {
                        var datum = buffer[i];
                        var dentity = datum.Type switch {
                            DatumType.Double => doubles[datum.ID],
                            DatumType.String64 => string64s[datum.ID],
                            DatumType.String512 => string512s[datum.ID],
                            _ => throw new System.Exception($"unknown DatumType: {datum.Type}"),
                        };
                        nbuffer.Add(new DatumRefBuffer {
                                Entity = dentity,
                                Type = datum.Type,
                            });
                    }
                    ecb.RemoveComponent<UninitializedDatumRefBuffer>(entity);
                })
                .Schedule();

            // finish
            this.Dependency.Complete();
            ecb.Playback(this.EntityManager);
            ecb.Dispose();
            new_doubles.Dispose();
            new_string64s.Dispose();
            new_string512s.Dispose();
        }
    }
}
