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
        private NativeParallelHashMap<FixedString64Bytes, Entity> Strings;
        private EntityQuery DatumRefQuery;
        private EntityQuery DatumRefBufferQuery;

        [BurstCompile]
        protected override void OnCreate() {
            var N = 1000;
            this.Doubles = new NativeParallelHashMap<FixedString64Bytes, Entity>(N, Allocator.Persistent);
            this.Strings = new NativeParallelHashMap<FixedString64Bytes, Entity>(N, Allocator.Persistent);
            RequireForUpdate(DatumRefQuery);
            RequireForUpdate(DatumRefBufferQuery);
        }
        
        [BurstCompile]
        protected override void OnUpdate() {
            var new_doubles = new NativeHashSet<FixedString64Bytes>(64, Allocator.TempJob);
            var new_strings = new NativeHashSet<FixedString64Bytes>(64, Allocator.TempJob);
            var doubles = this.Doubles;
            var strings = this.Strings;

            // load new DatumDoubles
            var job_doubles = Entities
                .WithChangeFilter<DatumDouble>()
                .ForEach((Entity entity, in DatumDouble datum) => {
                    doubles[datum.ID] = entity;
                })
                .Schedule(this.Dependency);

            // load new DatumString64s
            var job_strings = Entities
                .WithChangeFilter<DatumString64>()
                .ForEach((Entity entity, in DatumString64 datum) => {
                    strings[datum.ID] = entity;
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
                            new_strings.Add(datum.ID); break;
                        default:
                            throw new System.Exception($"unknown DatumType: {datum.Type}");
                    }
                })
                .Schedule(this.Dependency);
            
            // find UninitializedDatumRefBufferss
            var job_finds = Entities
                .WithStoreEntityQueryInField(ref DatumRefBufferQuery)
                .ForEach((in DynamicBuffer<UninitializedDatumRefBuffer> buffer) => {
                    for (int i=0; i<buffer.Length; i++) {
                        var datum = buffer[i];
                        switch (datum.Type) {
                            case DatumType.Double:
                                new_doubles.Add(datum.ID); break;
                            case DatumType.String64:
                                new_strings.Add(datum.ID); break;
                            default:
                                throw new System.Exception($"unknown DatumType: {datum.Type}");
                        }
                    }
                })
                .Schedule(job_find_datumrefs);

            var job_loads = JobHandle.CombineDependencies(job_doubles, job_strings);
            this.Dependency = JobHandle.CombineDependencies(job_loads, job_finds);
            this.Dependency.Complete();

            // create new Datum entities
            foreach (var ID in new_doubles) {
                var entity = EntityManager.CreateEntity();
                EntityManager.AddComponentData<DatumDouble>(entity, new DatumDouble { ID=ID });
                doubles[ID] = entity;
            }
            foreach (var ID in new_strings) {
                var entity = EntityManager.CreateEntity();
                EntityManager.AddComponentData<DatumString64>(entity, new DatumString64 { ID=ID });
                strings[ID] = entity;
            }

            // rest will use an ecb
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            
            // realize DatumRefs
            Entities
                .ForEach((Entity entity, in UninitializedDatumRef datum) => {
                    var dentity = (datum.Type == DatumType.Double) ? doubles[datum.ID] : strings[datum.ID];
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
                        var dentity = (datum.Type == DatumType.Double) ? doubles[datum.ID] : strings[datum.ID];
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
            new_strings.Dispose();
        }
    }
}
