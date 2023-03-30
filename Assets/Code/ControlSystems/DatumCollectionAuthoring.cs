#if UNITY_EDITOR
using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using Unity.Collections;
using Unity.Entities;

namespace Icarus.UI {
    public class DatumCollectionAuthoring : MonoBehaviour {
        public UnityEngine.Object DatumDouble;
        public UnityEngine.Object DatumString64;
        public UnityEngine.Object DatumString512;
        
        public class DatumCollectionAuthoringBaker : Baker<DatumCollectionAuthoring> {
            public override void Bake(DatumCollectionAuthoring auth) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                DependsOn(auth.DatumDouble);
                DependsOn(auth.DatumString64);
                DependsOn(auth.DatumString512);
                var datums = new DatumCollection(Allocator.Persistent);
                foreach (var (name, value) in ParseFile(auth.DatumDouble)) {
                    datums.SetDouble(name, double.Parse(value));
                }
                foreach (var (name, value) in ParseFile(auth.DatumString64)) {
                    datums.SetString64(name, value);
                }
                foreach (var (name, value) in ParseFile(auth.DatumString512)) {
                    datums.SetString512(name, value);
                }
                AddComponent(entity, datums);
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
}
#endif
