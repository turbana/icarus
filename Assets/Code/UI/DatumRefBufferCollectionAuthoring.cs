using UnityEngine;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace Icarus.UI {
    public class DatumRefBufferCollectionAuthoring : MonoBehaviour {
        public string[] ExtraDatums;
        public DatumType[] ExtraDatumTypes;
        
        public class DatumRefBufferCollectionAuthoringBaker
            : Baker<DatumRefBufferCollectionAuthoring> {
            public override void Bake(DatumRefBufferCollectionAuthoring auth) {
                var dcount = (auth.ExtraDatums is null) ? -1 : auth.ExtraDatums.Length;
                var tcount = (auth.ExtraDatumTypes is null) ? -1 : auth.ExtraDatumTypes.Length;
                if (dcount != tcount) {
                    Debug.LogError($"The count of ExtraDatums must match ExtraDatumTypes");
                    return;
                }
                var buffers = new UnsafeList<UninitializedDatumRefBuffer>
                    (dcount, Allocator.Persistent);
                buffers.Length = dcount;
                for (int i=0; i<dcount; i++) {
                    buffers[i] = new UninitializedDatumRefBuffer {
                        ID = auth.ExtraDatums[i],
                        Type = auth.ExtraDatumTypes[i]
                    };
                }
                // Debug.Log($"finding children under {auth.gameObject}");
                var children = new UnsafeList<Entity>(10, Allocator.Persistent);
                GetChildren(auth.gameObject, ref children);
                // Debug.Log($"found {children.Length} children");
                AddComponent<DatumRefBufferCollector>(new DatumRefBufferCollector {
                        Children = children,
                        ExtraBuffers = buffers,
                    });
            }

            private void GetChildren(GameObject go, ref UnsafeList<Entity> children) {
                for (int i=0; i<go.transform.childCount; i++) {
                    var child = go.transform.GetChild(i).gameObject;
                    // Debug.Log($"child found: {child}");
                    children.Add(GetEntity(child));
                    GetChildren(child, ref children);
                }
            }
        }
    }

    /* We want to collect all DatumRef(Buffer)s on this Entity and any of it's
     * children. Those components are added during the baking process so we
     * need to run a baking system to collect them. */
    [WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
    partial class DatumRefBufferCollectionAuthoringBakingSystem : SystemBase {
        protected ComponentLookup<UninitializedDatumRef> DatumRefLookup;
        protected BufferLookup<UninitializedDatumRefBuffer> DatumRefBufferLookup;
        
        protected override void OnCreate() {
            DatumRefLookup = GetComponentLookup<UninitializedDatumRef>();
            DatumRefBufferLookup = GetBufferLookup<UninitializedDatumRefBuffer>();
        }
        
        protected override void OnUpdate() {
            DatumRefLookup.Update(this);
            DatumRefBufferLookup.Update(this);
            var DRL = DatumRefLookup;
            var DRBL = DatumRefBufferLookup;
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            var refs = new UnsafeList<UninitializedDatumRefBuffer>(10, Allocator.TempJob);

            Entities
                .ForEach((Entity entity, ref DatumRefBufferCollector collector) => {
                    foreach (var  buffer in collector.ExtraBuffers) {
                        refs.Add(buffer);
                    }
                    foreach (var child in collector.Children) {
                        if (DRL.HasComponent(child)) {
                            var datum = DRL[child];
                            refs.Add(new UninitializedDatumRefBuffer {
                                    ID = datum.ID,
                                    Type = datum.Type,
                                });
                        }
                        if (DRBL.HasBuffer(child)) {
                            foreach (var buf in DRBL[child]) {
                                refs.Add(buf);
                            }
                        }
                    }
                    // Debug.Log($"searched {collector.Children.Length} entities and found {refs.Length} refs");
                    ecb.RemoveComponent<DatumRefBufferCollector>(entity);
                    if (refs.Length > 0) {
                        // add components
                        var map = new NativeHashMap<FixedString64Bytes, int>(refs.Length, Allocator.Persistent);
                        var buffer = ecb.AddBuffer<UninitializedDatumRefBuffer>(entity);
                        for (int i=0; i<refs.Length; i++) {
                            var datum = refs[i];
                            buffer.Add(datum);
                            map[datum.ID] = i;
                        }
                        ecb.AddComponent<DatumRefBufferCollection>(entity, new DatumRefBufferCollection {
                                IndexMap = map,
                            });
                    } else {
                        Debug.LogWarning($"no DatumRef(Buffers) found in any children for {entity}");
                    }
                })
                .Schedule();

            this.Dependency.Complete();
            ecb.Playback(this.EntityManager);
            ecb.Dispose();
            refs.Dispose();
        }
    }
}
