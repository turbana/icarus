using UnityEditor;
using UnityEngine;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Icarus.Orbit {
    public struct OrbitalDatabaseComponent : IComponentData {
        public NativeHashMap<FixedString64Bytes, Entity> EntityMap;
        public NativeHashMap<FixedString64Bytes, OrbitalDatabaseData> DataMap;
    }

    public struct OrbitalDatabaseData {
        public FixedString64Bytes Name;
        public FixedString32Bytes Type;
        public FixedString64Bytes Parent;
        public float Radius;
        public float Period;
        public float Eccentricity;
        public float SemiMajorAxis;
        public float Inclination;
        public float AscendingNode;
        public float ElapsedTime;
        public float AxialTilt;
        public float NorthPoleRA;
        public float RotationPeriod;
        public float RotationElapsedTime;

        public override string ToString() => $"<database-data Name={Name} Type={Type} Parent={Parent} Radius={Radius} Period={Period} Eccentricity={Eccentricity} SemiMajorAxis={SemiMajorAxis} Inclination={Inclination} AscendingNode={AscendingNode} ElapsedTime={ElapsedTime} AxialTilt={AxialTilt} NorthPoleRA={NorthPoleRA} RotationPeriod={RotationPeriod} RotationElapsedTime={RotationElapsedTime}>";
    }

    [AddComponentMenu("Icarus/Orbit/Orbital Database")]
    public class OrbitalDatabaseAuthoring : MonoBehaviour {
        public Object Database;
        
        public class OrbitalDatabaseAuthoringBaker : Baker<OrbitalDatabaseAuthoring> {
            public override void Bake(OrbitalDatabaseAuthoring auth) {
                DependsOn(auth.Database);
                var orbits = ParseDatabase(AssetDatabase.GetAssetPath(auth.Database));

                var emap = new NativeHashMap<FixedString64Bytes, Entity>
                    (orbits.Length, Allocator.Persistent);
                var dmap = new NativeHashMap<FixedString64Bytes, OrbitalDatabaseData>
                    (orbits.Length, Allocator.Persistent);

                foreach (var data in orbits) {
                    dmap[data.Name] = data;
                }

                AddComponent<OrbitalDatabaseComponent>(new OrbitalDatabaseComponent {
                        EntityMap = emap,
                        DataMap = dmap
                    });
            }
        }

        private static OrbitalDatabaseData[] ParseDatabase(string path) {
            string[] lines = System.IO.File.ReadAllLines(path);
            // ignore csv header line
            var data = new OrbitalDatabaseData[lines.Length - 1];

            for (int i=0; i<data.Length; i++) {
                string[] line = lines[i+1].Split(',');
                data[i] = new OrbitalDatabaseData {
                    Name = line[0],
                    Type = line[1],
                    Parent = line[2]
                };
                float.TryParse(line[3], out data[i].Radius);
                float.TryParse(line[4], out data[i].Period);
                float.TryParse(line[5], out data[i].Eccentricity);
                float.TryParse(line[6], out data[i].SemiMajorAxis);
                float.TryParse(line[7], out data[i].Inclination);
                float.TryParse(line[8], out data[i].AscendingNode);
                float.TryParse(line[9], out data[i].ElapsedTime);
                float.TryParse(line[10], out data[i].AxialTilt);
                float.TryParse(line[11], out data[i].NorthPoleRA);
                float.TryParse(line[12], out data[i].RotationPeriod);
                float.TryParse(line[13], out data[i].RotationElapsedTime);
                // check for 0-length Periods and set them to a SemiMajorAxisll value;
                if (data[i].Period == 0f) data[i].Period = 1f;
                if (data[i].RotationPeriod == 0f) data[i].RotationPeriod = 1f;
            }
            
            return data;
        }
    }
}
