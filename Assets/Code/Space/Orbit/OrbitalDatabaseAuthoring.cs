using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

using Random = Unity.Mathematics.Random;

namespace Icarus.Orbit {
    using BodyName = FixedString64Bytes;
    using OrbitalDatabase = NativeHashMap<FixedString64Bytes, OrbitalDatabaseData>;
    using EntityDatabase = NativeHashMap<FixedString64Bytes, Entity>;
    
    public struct OrbitalDatabaseDataComponent : IBufferElementData {
        public FixedString64Bytes Name;
        public Entity Value;
    }
    
    public struct OrbitalDatabaseComponent : IComponentData {
        private static FixedString64Bytes DEFAULT_PREFAB = "Planet Prefab";

        private OrbitalDatabase DataMap;
        private EntityDatabase EntityMap;
        private EntityDatabase PrefabMap;

        public OrbitalDatabaseComponent(OrbitalDatabase dmap) {
            this.DataMap = dmap;
            this.EntityMap = new NativeHashMap<FixedString64Bytes, Entity>(0, Allocator.Persistent);
            this.PrefabMap = new NativeHashMap<FixedString64Bytes, Entity>(0, Allocator.Persistent);
        }

        public int DataCount {
            get => DataMap.Count;
        }

        public int EntityCount {
            get => EntityMap.Count;
        }

        public int PrefabCount {
            get => PrefabMap.Count;
        }

        public int EntityCapacity {
            get => EntityMap.Capacity;
            set => EntityMap.Capacity = value;
        }

        public int PrefabCapacity {
            get => PrefabMap.Capacity;
            set => PrefabMap.Capacity = value;
        }

        public OrbitalDatabaseData LookupData(BodyName name) {
            return DataMap[name];
        }

        public Entity LookupEntity(BodyName name) {
            return EntityMap[name];
        }

        public void SaveEntity(BodyName name, Entity entity) {
            EntityMap[name] = entity;
        }

        public Entity LookupPrefab(BodyName name) {
            var key = PrefabMap.ContainsKey(name) ? name : DEFAULT_PREFAB;
            return PrefabMap[key];
        }

        public void SavePrefab(BodyName name, Entity prefab) {
            PrefabMap[name] = prefab;
        }

        public NativeArray<BodyName> GetDataKeyArray(AllocatorManager.AllocatorHandle allocator) {
            return DataMap.GetKeyArray(allocator);
        }

        public NativeArray<BodyName> GetPrefabKeys(AllocatorManager.AllocatorHandle allocator) {
            return PrefabMap.GetKeyArray(allocator);
        }
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

        public override string ToString() => $"<database-data Name=\"{Name}\" Type={Type} Parent=\"{Parent}\" Radius={Radius} Period={Period} Eccentricity={Eccentricity} SemiMajorAxis={SemiMajorAxis} Inclination={Inclination} AscendingNode={AscendingNode} ElapsedTime={ElapsedTime} AxialTilt={AxialTilt} NorthPoleRA={NorthPoleRA} RotationPeriod={RotationPeriod} RotationElapsedTime={RotationElapsedTime}>";
    }

    [AddComponentMenu("Icarus/Orbit/Orbital Database")]
    public class OrbitalDatabaseAuthoring : MonoBehaviour {
        public uint RandomSeed = 1;
        public Object PrefabPath;
        [Space]
        public Object CustomDatabase;
        public Object SatelliteDatabase;
        public Object SmallBodiesDatabase;
        public Object SmallBodiesTopDatabase;
        [Space]
        public bool IncludeCustomDatabase;
        public bool IncludeSatelliteDatabase;
        public bool IncludeSmallBodiesDatabase;
        public bool IncludeSmallBodiesTopDatabase;
        [Space]
        public bool FiddleToReloadDatabase;

        // these objects are considered dwarf planets
        // https://en.wikipedia.org/wiki/Dwarf_planet#Population_of_dwarf_planets
        private static string[] DWARF_PLANETS = new string[] {
            "1 Ceres (A801 AA)",
            "134340 Pluto (1930 BM)",
            "136199 Eris (2003 UB313)",
            "136108 Haumea (2003 EL61)",
            "136472 Makemake (2005 FY9)",
            "50000 Quaoar (2002 LM60)",
            "90377 Sedna (2003 VB12)",
            "90482 Orcus (2004 DW)",
            "225088 Gonggong (2007 OR10)"
        };

        public class OrbitalDatabaseAuthoringBaker : Baker<OrbitalDatabaseAuthoring> {
            public override void Bake(OrbitalDatabaseAuthoring auth) {
                DependsOn(auth.CustomDatabase);
                DependsOn(auth.SatelliteDatabase);
                DependsOn(auth.SmallBodiesDatabase);
                DependsOn(auth.SmallBodiesTopDatabase);

                string CustomDatabasePath = AssetDatabase.GetAssetPath(auth.CustomDatabase);
                string SatelliteDatabasePath = AssetDatabase.GetAssetPath(auth.SatelliteDatabase);
                string SmallBodiesDatabasePath = AssetDatabase.GetAssetPath(auth.SmallBodiesDatabase);
                string SmallBodiesTopDatabasePath = AssetDatabase.GetAssetPath(auth.SmallBodiesTopDatabase);
                var rand = new Random(auth.RandomSeed);

                var GuessSize = 100;

                var dmap = new OrbitalDatabase(GuessSize, Allocator.Persistent);
                var emap = new EntityDatabase(GuessSize, Allocator.Persistent);
                var pmap = new EntityDatabase(GuessSize, Allocator.Persistent);

                if (auth.IncludeCustomDatabase) {
                    LoadCustomDatabase(dmap, CustomDatabasePath);
                }
                if (auth.IncludeSatelliteDatabase) {
                    LoadSatelliteDatabase(dmap, SatelliteDatabasePath);
                }
                if (auth.IncludeSmallBodiesDatabase) {
                    Debug.Log("Loading Small Bodies database, this may take a while...");
                    LoadSmallBodiesDatabase(dmap, SmallBodiesDatabasePath);
                    Debug.Log("Loaded Small Bodies database");
                }
                if (auth.IncludeSmallBodiesTopDatabase) {
                    LoadSmallBodiesDatabase(dmap, SmallBodiesTopDatabasePath);
                }

                this.SetPrefabs(pmap, auth.PrefabPath);
                FixupData(dmap, rand);
                AssertData(dmap);
                ShowStatistics(dmap, pmap);

                var comp = new OrbitalDatabaseComponent(dmap);
                AddComponent<OrbitalDatabaseComponent>(comp);
            }

            private void SetPrefabs(EntityDatabase pmap, Object dir) {
                string[] root = new string[] { AssetDatabase.GetAssetPath(dir) };
                string[] assets = AssetDatabase.FindAssets("t:GameObject", root);
                var prefabs = AddBuffer<OrbitalDatabaseDataComponent>();
                for (int i=0; i<assets.Length; i++) {
                    string path = AssetDatabase.GUIDToAssetPath(assets[i]);
                    GameObject prefab = AssetDatabase.LoadMainAssetAtPath(path) as GameObject;
                    var comp = new OrbitalDatabaseDataComponent {
                        Name = prefab.name, Value = GetEntity(prefab) };
                    prefabs.Add(comp);
                    pmap[prefab.name] = GetEntity(prefab);
                }
            }
        }

        private static void ShowStatistics(OrbitalDatabase db, EntityDatabase pmap) {
            var bodies = db.GetValueArray(Allocator.TempJob);
            int planets = -1;   // don't count the sun even though it's set as a planet
            int moons = 0;
            int asteroids = 0;
            int dwarfplanets = 0;
            int ships = 0;
            int players = 0;
            int prefabs = pmap.Count;
            
            foreach (var body in bodies) {
                if (body.Type == "Planet") planets += 1;
                else if (body.Type == "Moon") moons += 1;
                else if (body.Type == "Asteroid") asteroids += 1;
                else if (body.Type == "DwarfPlanet") dwarfplanets += 1;
                else if (body.Type == "Ship") ships += 1;
                else if (body.Type == "Player") players += 1;
                else throw new System.Exception($"Invalid type for {body}");
            }

            Debug.Log($"Orbital database now contains {bodies.Length} total bodies ({planets} planets, {moons} moons, {dwarfplanets} dwarf planets, {asteroids} asteroids, {ships} ships, {players} players) and {prefabs} prefabs");
            bodies.Dispose();
        }

        private static void AddBody(OrbitalDatabase db, OrbitalDatabaseData body) {
            var name = body.Name;
            if (db.ContainsKey(name)) {
                body = MergeBody(body, db[name]);
            }
            db[name] = body;
        }
        
        private static OrbitalDatabaseData MergeBody(OrbitalDatabaseData left, OrbitalDatabaseData right) {
            left.Radius = float.IsNaN(left.Radius) ? right.Radius : left.Radius;
            left.Period = float.IsNaN(left.Period) ? right.Period : left.Period;
            left.Eccentricity = float.IsNaN(left.Eccentricity) ? right.Eccentricity : left.Eccentricity;
            left.SemiMajorAxis = float.IsNaN(left.SemiMajorAxis) ? right.SemiMajorAxis : left.SemiMajorAxis;
            left.Inclination = float.IsNaN(left.Inclination) ? right.Inclination : left.Inclination;
            left.AscendingNode = float.IsNaN(left.AscendingNode) ? right.AscendingNode : left.AscendingNode;
            left.ElapsedTime = float.IsNaN(left.ElapsedTime) ? right.ElapsedTime : left.ElapsedTime;
            left.AxialTilt = float.IsNaN(left.AxialTilt) ? right.AxialTilt : left.AxialTilt;
            left.NorthPoleRA = float.IsNaN(left.NorthPoleRA) ? right.NorthPoleRA : left.NorthPoleRA;
            left.RotationPeriod = float.IsNaN(left.RotationPeriod) ? right.RotationPeriod : left.RotationPeriod;
            left.RotationElapsedTime = float.IsNaN(left.RotationElapsedTime) ? right.RotationElapsedTime : left.RotationElapsedTime;
            return left;
        }

        private static IEnumerable<string[]> ReadCsv(string path) {
            bool skip = true;
            foreach (var line in System.IO.File.ReadLines(path)) {
                if (skip) {
                    skip = false;
                } else {
                    yield return line.Split(',');
                }
            }
        }

        private static void LoadCustomDatabase(OrbitalDatabase db, string path) {
            foreach (var line in ReadCsv(path)) {
                var data = new OrbitalDatabaseData {
                    Name = line[0],
                    Type = line[1],
                    Parent = line[2]
                };
                data.Radius = ParseFloat(line[3]);
                data.Period = ParseFloat(line[4]);
                data.Eccentricity = ParseFloat(line[5]);
                data.SemiMajorAxis = ParseFloat(line[6]);
                data.Inclination = ParseFloat(line[7]);
                data.AscendingNode = ParseFloat(line[8]);
                data.ElapsedTime = ParseFloat(line[9]);
                data.AxialTilt = ParseFloat(line[10]);
                data.NorthPoleRA = ParseFloat(line[11]);
                data.RotationPeriod = ParseFloat(line[12]);
                data.RotationElapsedTime = ParseFloat(line[13]);
                // check for 0-length Periods and set them to a small value;
                if (data.Period == 0f) data.Period = 1f;
                if (data.RotationPeriod == 0f) data.RotationPeriod = 1f;
                AddBody(db, data);
            }
        }

        private static void LoadSatelliteDatabase(OrbitalDatabase db, string path) {
            foreach (var line in ReadCsv(path)) {
                var data = new OrbitalDatabaseData {
                    Name = line[1],
                    Type = "Moon",
                    Parent = line[0]
                };
                data.Radius = float.NaN;
                data.Period = ParseFloat(line[12]) * 24f * 60f * 60f;
                data.Eccentricity = ParseFloat(line[7]);
                data.SemiMajorAxis = ParseFloat(line[6]);
                data.Inclination = ParseFloat(line[10]);
                data.AscendingNode = ParseFloat(line[11]);
                data.ElapsedTime = float.NaN;
                data.AxialTilt = ParseFloat(line[17]);
                data.NorthPoleRA = ParseFloat(line[15]);
                data.RotationPeriod = float.NaN;
                data.RotationElapsedTime = float.NaN;
                // TODO find inclination in parent-equator plane
                AddBody(db, data);
            }
        }

        private static void LoadSmallBodiesDatabase(OrbitalDatabase db, string path) {
            foreach (var line in ReadCsv(path)) {
                var data = new OrbitalDatabaseData {
                    Name = line[1].Trim(new char[] {' ', '"'}),
                    Type = "Asteroid", // assume asteroid
                    Parent = "Sun"
                };
                data.Radius = ParseFloat(line[10]) / 2f;
                data.Period = ParseFloat(line[4]) * 24f * 60f * 60f;
                data.Eccentricity = ParseFloat(line[5]);
                data.SemiMajorAxis = ParseFloat(line[6]);
                data.Inclination = ParseFloat(line[7]);
                data.AscendingNode = ParseFloat(line[8]);
                data.ElapsedTime = float.NaN;
                data.AxialTilt = float.NaN;
                data.NorthPoleRA = float.NaN;
                data.RotationPeriod = ParseFloat(line[9]) * 60f * 60f;
                data.RotationElapsedTime = float.NaN; // TODO in data with epoch
                if (System.Array.IndexOf(DWARF_PLANETS, data.Name.ToString()) >= 0) {
                    data.Type = "DwarfPlanet";
                }
                AddBody(db, data);
            }
        }

        private static float ParseFloat(string str) {
            float data;
            if (!float.TryParse(str, out data)) {
                data = float.NaN;
            }
            return data;
        }

        private static void FixupData(OrbitalDatabase db, Random rand) {
            var names = db.GetKeyArray(Allocator.TempJob);
            foreach (var name in names) {
                var body = db[name];
                if (float.IsNaN(body.Radius)) body.Radius = rand.NextFloat(0.05f, 1f);
                if (float.IsNaN(body.ElapsedTime)) body.ElapsedTime = 0f;
                if (float.IsNaN(body.AxialTilt)) body.AxialTilt = rand.NextFloat(-180f, 180f);
                if (float.IsNaN(body.NorthPoleRA)) body.NorthPoleRA = rand.NextFloat(-180f, 180f);
                if (float.IsNaN(body.RotationPeriod)) body.RotationPeriod = rand.NextFloat(1000f, 100000f);
                if (float.IsNaN(body.RotationElapsedTime)) body.RotationElapsedTime = 0f;
                db[name] = body;
            }
            names.Dispose();
        }

        private static void AssertData(OrbitalDatabase db) {
            var names = db.GetKeyArray(Allocator.TempJob);
            foreach (var name in names) {
                var body = db[name];
                bool hasNaN =
                    float.IsNaN(body.Radius) ||
                    float.IsNaN(body.Period) ||
                    float.IsNaN(body.Eccentricity) ||
                    float.IsNaN(body.SemiMajorAxis) ||
                    float.IsNaN(body.Inclination) ||
                    float.IsNaN(body.AscendingNode) ||
                    float.IsNaN(body.ElapsedTime) ||
                    float.IsNaN(body.AxialTilt) ||
                    float.IsNaN(body.NorthPoleRA) ||
                    float.IsNaN(body.RotationPeriod) ||
                    float.IsNaN(body.RotationElapsedTime);
                bool hasParent = db.ContainsKey(body.Parent);
                Debug.Assert(!hasNaN, $"NaN detected in {body}");
                Debug.Assert(hasParent, $"Parent not found in {body}");
                db[name] = body;
            }
            names.Dispose();
        }
    }
}
