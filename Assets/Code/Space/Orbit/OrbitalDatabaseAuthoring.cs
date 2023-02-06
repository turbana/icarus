using System.Collections.Generic;
using System.IO;
using System.Text;

using UnityEditor;
using UnityEngine;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;


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

    [System.Serializable]
    public struct OrbitalDatabaseData {
        public FixedString64Bytes Name;
        public FixedString32Bytes Type;
        public FixedString64Bytes Parent;
        public float Radius;
        public double Mass;
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

        public override string ToString() => $"<database-data Name=\"{Name}\" Type={Type} Parent=\"{Parent}\" Radius={Radius} Mass={Mass} Period={Period} Eccentricity={Eccentricity} SemiMajorAxis={SemiMajorAxis} Inclination={Inclination} AscendingNode={AscendingNode} ElapsedTime={ElapsedTime} AxialTilt={AxialTilt} NorthPoleRA={NorthPoleRA} RotationPeriod={RotationPeriod} RotationElapsedTime={RotationElapsedTime}>";

        public void StreamWrite(BinaryWriter writer) {
            writer.Write(Name.ToString());
            writer.Write(Type.ToString());
            writer.Write(Parent.ToString());
            writer.Write(Radius);
            writer.Write(Mass);
            writer.Write(Period);
            writer.Write(Eccentricity);
            writer.Write(SemiMajorAxis);
            writer.Write(Inclination);
            writer.Write(AscendingNode);
            writer.Write(ElapsedTime);
            writer.Write(AxialTilt);
            writer.Write(NorthPoleRA);
            writer.Write(RotationPeriod);
            writer.Write(RotationElapsedTime);
        }

        public void StreamRead(BinaryReader reader) {
            Name = new FixedString64Bytes(reader.ReadString());
            Type = new FixedString32Bytes(reader.ReadString());
            Parent = new FixedString64Bytes(reader.ReadString());
            Radius = reader.ReadSingle();
            Mass = reader.ReadDouble();
            Period = reader.ReadSingle();
            Eccentricity = reader.ReadSingle();
            SemiMajorAxis = reader.ReadSingle();
            Inclination = reader.ReadSingle();
            AscendingNode = reader.ReadSingle();
            ElapsedTime = reader.ReadSingle();
            AxialTilt = reader.ReadSingle();
            NorthPoleRA = reader.ReadSingle();
            RotationPeriod = reader.ReadSingle();
            RotationElapsedTime = reader.ReadSingle();
        }
    }

    [AddComponentMenu("Icarus/Orbit/Orbital Database")]
    public class OrbitalDatabaseAuthoring : MonoBehaviour {
        [Tooltip("The seed used to generate any random parameters (asteroid mass, composition, etc)")]
        public uint RandomSeed = 1;
        [Tooltip("The root path for prefab assets")]
        public Object PrefabPath;
        [Tooltip("The final orbital database")]
        public Object OrbitalDatabase;
        [Space]
        [Tooltip("Custom orbital database")]
        public Object CustomDatabase;
        [Tooltip("SDDB Satellite database")]
        public Object SatelliteDatabase;
        [Tooltip("SDDB Small Bodies database")]
        public Object SmallBodiesDatabase;

        public OrbitalDatabaseData LookupBody(BodyName body) {
            var db = new OrbitalDatabase(100, Allocator.TempJob);
            LoadDatabase(db);
            var data = db[body];
            db.Dispose();
            return data;
        }

        public void SaveDatabase(OrbitalDatabase db) {
            string path =
                AssetDatabase.GetAssetPath(this.OrbitalDatabase);
            var values = db.GetValueArray(Allocator.TempJob);
            var encoder = new UTF8Encoding(true, true);
            using (var stream = File.Open(path, FileMode.Create)) {
                using (var writer = new BinaryWriter(stream, Encoding.UTF8, false)) {
                    for (int i=0; i<values.Length; i++) {
                        values[i].StreamWrite(writer);
                    }
                }
            }
            values.Dispose();
            EditorUtility.SetDirty(this);
        }

        public void LoadDatabase(OrbitalDatabase db) {
            string path =
                AssetDatabase.GetAssetPath(this.OrbitalDatabase);
            using (var stream = File.Open(path, FileMode.Open)) {
                using (var reader = new BinaryReader(stream, Encoding.UTF8, false)) {
                    while (reader.PeekChar() != -1) {
                        var data = new OrbitalDatabaseData();
                        data.StreamRead(reader);
                        db.Add(data.Name, data);
                    }
                }
            }
        }

        public class OrbitalDatabaseAuthoringBaker : Baker<OrbitalDatabaseAuthoring> {
            public override void Bake(OrbitalDatabaseAuthoring auth) {
                DependsOn(auth.OrbitalDatabase);

                var GuessSize = 100;

                var dmap = new OrbitalDatabase(GuessSize, Allocator.Persistent);
                var emap = new EntityDatabase(GuessSize, Allocator.Persistent);
                var pmap = new EntityDatabase(GuessSize, Allocator.Persistent);

                auth.LoadDatabase(dmap);
                SetPrefabs(pmap, auth.PrefabPath);
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
    }
}
