using System.Collections.Generic;
using Random = Unity.Mathematics.Random;

using UnityEditor;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;

using Icarus.Orbit;

namespace Icarus.Orbit.Editor {
    using BodyName = FixedString64Bytes;
    using OrbitalDatabase = NativeHashMap<FixedString64Bytes, OrbitalDatabaseData>;
    using EntityDatabase = NativeHashMap<FixedString64Bytes, Entity>;

    [CustomEditor(typeof(OrbitalDatabaseAuthoring))]
    public class OrbitalDatabaseAuthoringEditor : UnityEditor.Editor {
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

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            if (GUILayout.Button("Build Database")) {
                BuildDatabase();
            }
        }

        private void BuildDatabase() {
            Debug.Log("Starting to build orbital database");
            var script = target as OrbitalDatabaseAuthoring;
            string OrbitalDatabasePath =
                AssetDatabase.GetAssetPath(script.OrbitalDatabase);
            string CustomDatabasePath =
                AssetDatabase.GetAssetPath(script.CustomDatabase);
            string SatelliteDatabasePath
                = AssetDatabase.GetAssetPath(script.SatelliteDatabase);
            string SmallBodiesDatabasePath
                = AssetDatabase.GetAssetPath(script.SmallBodiesDatabase);
            
            var rand = new Random(script.RandomSeed);
            var db = new OrbitalDatabase(1000, Allocator.Persistent);
            
            LoadCustomDatabase(db, CustomDatabasePath);
            LoadSatelliteDatabase(db, SatelliteDatabasePath);
            LoadSmallBodiesDatabase(db, SmallBodiesDatabasePath);
            
            FixupData(db, rand);
            AssertData(db);
            script.SaveDatabase(db);
            db.Dispose();
            Debug.Log("Finished building orbital database");
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
            left.Mass = float.IsNaN(left.Mass) ? right.Mass : left.Mass;
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
                data.Mass = float.NaN;
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
                data.Mass = float.NaN;
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
                data.Mass = float.NaN;
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
                    float.IsNaN(body.Mass) ||
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
