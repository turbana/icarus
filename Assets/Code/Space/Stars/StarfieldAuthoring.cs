using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;

using Unity.Collections;
using UnityEditor;
#endif

namespace Icarus.Space {
    public struct StarfieldComponent : IComponentData {
        public Entity Prefab;
        public int Count;
    }

    public struct StarSetup : IBufferElementData {
        public LocalTransform Position;
        public float4 Color;
    }

#if UNITY_EDITOR
    [AddComponentMenu("Icarus/Space/Starfield")]
    public class StarfieldAuthoring : MonoBehaviour {
        public float Distance;
        public Object Catalog;
        public GameObject Prefab;
        
        public class StarfieldAuthoringBaker : Baker<StarfieldAuthoring> {
            private const float DEGREES_PER_HOUR = 360f / 24f;
            private struct StarData {
                public string name;
                public float ra;
                public float dec;
                public float mag;
                public float temp;
                
                public override string ToString() => $"{name} ({ra}ra {dec}dec) {mag}mag {temp}K";
            }
    
            public override void Bake(StarfieldAuthoring auth) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                var catalog = AssetDatabase.GetAssetPath(auth.Catalog);
                var stars = new List<StarData>(ParseStars(catalog));
                var buffer = AddBuffer<StarSetup>(entity);

                // load star data
                foreach (var star in stars) {
                    var setup = SetupStar(star, auth.Distance);
                    buffer.Add(setup);
                }

                // bake main entity
                var prefab = GetEntity(auth.Prefab, TransformUsageFlags.Dynamic);
                RegisterPrefabForBaking(auth.Prefab);
                AddComponent(entity, new StarfieldComponent {
                        Prefab = prefab,
                        Count = stars.Count,
                    });
                
                // Debug.Log($"finished baking {stars.Count} stars");
            }

            private StarSetup SetupStar(in StarData star, float dist) {
                // see here for a magnitude visualization:
                // https://www.desmos.com/calculator/ekwgkaealx
                // find scale magnitude
                float smag = math.min(1f, math.atan(-star.mag / 2f + 1f) / 3f + 0.75f);
                // find alpha magnitude
                float amag = math.min(1f, math.sin(star.mag / 2.5f + 2.5f) / 1.5f + 0.8f);
                // find scale
                float scale = dist / 50f * smag;
                // find rotation
                quaternion rot =
                    quaternion.EulerXYZ(-math.radians(star.dec),
                                        -math.radians(star.ra * DEGREES_PER_HOUR),
                                        0f);
                // find position
                float3 pos = math.mul(rot, math.forward() * dist);
                // update local transform
                var transform = LocalTransform
                    .FromPositionRotationScale(pos, rot, scale);
                // find star color
                // scale the temperature closer to "white"
                float temp = star.temp + (5800f - star.temp) / 3f;
                Color color = Mathf.CorrelatedColorTemperatureToRGB(temp);
                return new StarSetup {
                    Position = transform,
                    Color = new float4(color.r, color.g, color.b, amag),
                };
            }
            
            private IEnumerable<StarData> ParseStars(string catalog) {
                string[] lines = System.IO.File.ReadAllLines(catalog);
                foreach (string line in lines) {
                    StarData star = new StarData();
                    star.name = line.Substring(14, 11).Trim();
                    if (star.name == "") {
                        continue;
                    }
                    // right ascension (hours, minutes, seconds)
                    float rah = float.Parse(line.Substring(75, 2));
                    float ram = float.Parse(line.Substring(77, 2));
                    float ras = float.Parse(line.Substring(79, 4));
                    // convert to hours
                    star.ra = (ras / 60f + ram) / 60f + rah;
                    // declination (sign, degrees, minutes, seconds)
                    string sign = line.Substring(83, 1);
                    float decd = float.Parse(line.Substring(84, 2));
                    float decm = float.Parse(line.Substring(86, 2));
                    float decs = float.Parse(line.Substring(88, 2));
                    // convert to degrees
                    star.dec = (decs / 60f + decm) / 60f + decd;
                    star.dec *= (sign == "-") ? -1f : 1f;
                    // magnitude
                    star.mag = float.Parse(line.Substring(102, 5));
                    // color temperature
                    float bv;
                    float.TryParse(line.Substring(109, 5), out bv);
                    // bv is now either set or 0
                    // https://en.wikipedia.org/wiki/Color_index
                    star.temp = 4600f * ((1 / (0.92f * bv + 1.7f)) +
                                         (1 / (0.92f * bv + 0.62f)));
                    yield return star;
                }
            }
        }
    }
#endif
}
