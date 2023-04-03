using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using Unity.Collections;
using Unity.Entities;

using Icarus.Loading;

namespace Icarus.UI {
    /* A DatumBlob holds the baked data for a single datum. */
    public struct DatumBlob {
        public BlobString ID;
        public double DoubleValue;
        public BlobString StringValue;
        public DatumType Type;
    }
    
    /* A DatumCollectionBlob holds all the DatumBlobs */
    public struct DatumCollectionBlob {
        public BlobArray<DatumBlob> DatumBlobs;
    }

    /* A BakedDatumCollection contains all the baked data for a DatumCollection. */
    public struct BakedDatumCollection : IComponentData {
        public BlobAssetReference<DatumCollectionBlob> Blob;
    }
    
#if UNITY_EDITOR
    public class DatumCollectionAuthoring : MonoBehaviour {
        public UnityEngine.Object DatumDouble;
        public UnityEngine.Object DatumString64;
        public UnityEngine.Object DatumString512;
    
        private struct DatumPreBlob {
            public string ID;
            public double DoubleValue;
            public string StringValue;
            public DatumType Type;
        }
        
        public class DatumCollectionAuthoringBaker : Baker<DatumCollectionAuthoring> {
            public override void Bake(DatumCollectionAuthoring auth) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                DependsOn(auth.DatumDouble);
                DependsOn(auth.DatumString64);
                DependsOn(auth.DatumString512);
                // var datums = new DatumCollection(Allocator.Persistent);
                List<DatumPreBlob> data = new List<DatumPreBlob>();
                foreach (var (name, value) in ParseFile(auth.DatumDouble)) {
                    // datums.SetDouble(name, double.Parse(value));
                    data.Add(new DatumPreBlob {
                            ID = name,
                            DoubleValue = double.Parse(value),
                            StringValue = "",
                            Type = DatumType.Double,
                        });
                }
                foreach (var (name, value) in ParseFile(auth.DatumString64)) {
                    // datums.SetString64(name, value);
                    data.Add(new DatumPreBlob {
                            ID = name,
                            StringValue = value,
                            Type = DatumType.String64,
                        });
                }
                foreach (var (name, value) in ParseFile(auth.DatumString512)) {
                    // datums.SetString512(name, value);
                    data.Add(new DatumPreBlob {
                            ID = name,
                            StringValue = value,
                            Type = DatumType.String512,
                        });
                }
                
                // build blob asset
                var builder = new BlobBuilder(Allocator.Temp);
                ref var collection = ref builder.ConstructRoot<DatumCollectionBlob>();
                BlobBuilderArray<DatumBlob> array =
                    builder.Allocate(ref collection.DatumBlobs, data.Count);

                // load datums array into blob
                for (int i=0; i<data.Count; i++) {
                    array[i] = new DatumBlob {
                        DoubleValue = data[i].DoubleValue,
                        Type = data[i].Type,
                    };
                    builder.AllocateString(ref array[i].ID, data[i].ID);
                    builder.AllocateString(ref array[i].StringValue, data[i].StringValue);
                }

                // add blob component
                var blob = builder.CreateBlobAssetReference<DatumCollectionBlob>(Allocator.Persistent);
                builder.Dispose();
                AddBlobAsset<DatumCollectionBlob>(ref blob, out var hash);
                AddComponent(entity, new BakedDatumCollection { Blob = blob });
            }

            public IEnumerable<Tuple<string, string>> ParseFile(UnityEngine.Object datafile) {
                var filename = AssetDatabase.GetAssetPath(datafile);
                var lines = System.IO.File.ReadLines(filename);
                foreach (var line in lines) {
                    var colon = line.IndexOf(":");
                    var name = line.Substring(0, colon).Trim();
                    var value = line.Substring(colon +1).Trim();
                    yield return new Tuple<string, string>(name, value);
                }
            }
        }
    }
#endif

    [UpdateInGroup(typeof(IcarusLoadingSystemGroup))]
    public partial class LoadDatumCollectionSystem : SystemBase {
        protected override void OnUpdate() {
            Entity entity;
            if (!SystemAPI.TryGetSingletonEntity<BakedDatumCollection>(out entity)) {
                // the datum collection is in a subscene that might not be
                // loaded yet.
                return;
            }
            var blob = SystemAPI.GetComponent<BakedDatumCollection>(entity);

            ref var blobs = ref blob.Blob.Value.DatumBlobs;
            var datums = new DatumCollection(Allocator.Persistent);

            UnityEngine.Debug.Log($"loading {blobs.Length} datums");
            for (int i=0; i<blobs.Length; i++) {
                var ID = blobs[i].ID.ToString();
                switch (blobs[i].Type) {
                    case DatumType.Double:
                        datums.SetDouble(ID, blobs[i].DoubleValue);
                        break;
                    case DatumType.String64:
                        datums.SetString64(ID, blobs[i].StringValue.ToString());
                        break;
                    case DatumType.String512:
                        datums.SetString512(ID, blobs[i].StringValue.ToString());
                        break;
                }
            }

            EntityManager.RemoveComponent<BakedDatumCollection>(entity);
            EntityManager.AddComponentData<DatumCollection>(entity, datums);
            
            // only run once
            this.Enabled = false;
        }
    }
}
