using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

using UnityEngine;

using Icarus.Orbit;
using Icarus.Mathematics;

namespace Icarus.Loading {
    public partial struct AddOrbitalParent : IComponentData {
        public FixedString64Bytes Value;
    }
    
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(IcarusLoadingSystemGroup))]
    public partial class LoadOrbitalBodySystem : SystemBase {
        private bool Loaded;
        protected static void LoadDatabase(in EntityCommandBuffer ecb, in Entity entity,
                                           in DynamicBuffer<OrbitalDatabaseData> data,
                                           in DynamicBuffer<OrbitalDatabasePrefab> prefabs) {
            var entities = new NativeArray<Entity>(data.Length, Allocator.TempJob);
            var database = new OrbitalDatabaseComponent(Allocator.Persistent);

            // load data
            database.DataCapacity = data.Length;
            for (int i=0; i<data.Length; i++) {
                database.SaveData(data[i]);
            }
            
            // load prefabs
            database.PrefabCapacity = prefabs.Length;
            for (int i=0; i<prefabs.Length; i++) {
                database.SavePrefab(prefabs[i].Name, prefabs[i].Prefab);
            }

            // initialize entity map
            database.EntityCapacity = database.DataCount;
            // Debug.Log($"setting ToLoad on {keys.Length} entities, using {database.PrefabMap.Count} prefabs");

            // instantiate and set orbital data
            for (int i=0; i<entities.Length; i++) {
                var name = data[i].Name;
                var prefab = database.LookupPrefab(name);
                entities[i] = ecb.Instantiate(prefab);
                // Debug.Log($"instantiated prefab for {name} -> {prefab} : {this.EntityManager.GetName(prefab)}");
                var comp = new OrbitalBodyToLoadComponent { Name = name };
                ecb.AddComponent<OrbitalBodyToLoadComponent>(entities[i], comp);
                ecb.RemoveComponent<Parent>(entities[i]);
            }

            ecb.AddComponent<OrbitalDatabaseComponent>(entity, database);
        }

        protected override void OnStartRunning() {
            Loaded = false;
        }
        
        protected override void OnUpdate() {
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            var entity = SystemAPI.GetSingletonEntity<OrbitalDatabaseTag>();
            
            if (!Loaded) {
                var data = SystemAPI.GetBuffer<OrbitalDatabaseData>(entity);
                var prefabs = SystemAPI.GetBuffer<OrbitalDatabasePrefab>(entity);
                
                LoadDatabase(ecb, entity, data, prefabs);
                ecb.RemoveComponent<OrbitalDatabaseData>(entity);
                ecb.RemoveComponent<OrbitalDatabasePrefab>(entity);
                
                ecb.Playback(this.EntityManager);
                ecb.Dispose();
                Loaded = true;
                
                // setup ecb for rest of job
                ecb = new EntityCommandBuffer(Allocator.TempJob);
            }
            
            var database = SystemAPI.GetSingletonRW<OrbitalDatabaseComponent>();

            new AddCoreOrbitalParametersJob {
                database = database.ValueRW,
                ecb = ecb
            }.Schedule();

            new AddOrbitalParentJob {
                database = database.ValueRO,
                ecb = ecb
            }.Schedule();

            new AddOrbitalParentJob2 {
                database = database.ValueRO,
                ecb = ecb
            }.Schedule();

            this.Dependency.Complete();
            ecb.Playback(this.EntityManager);
            ecb.Dispose();
        }

        
        public partial struct AddCoreOrbitalParametersJob : IJobEntity {
            public OrbitalDatabaseComponent database;
            public EntityCommandBuffer ecb;
            
            public void Execute(Entity entity, in OrbitalBodyToLoadComponent body) {
                // Debug.Log($"adding core to {body.Name}");
                AddOrbitalParameters(in entity, in body.Name, ref database, ref ecb);
            }
        }

        public partial struct AddOrbitalParentJob : IJobEntity {
            public OrbitalDatabaseComponent database;
            public EntityCommandBuffer ecb;
            
            public void Execute(Entity entity, ref OrbitalBodyToLoadComponent body) {
                var data = database.LookupData(body.Name);
                AddOrbitalParent(in entity, in body.Name, in database, ref ecb);
                ecb.RemoveComponent<OrbitalBodyToLoadComponent>(entity);
            }
        }

        public partial struct AddOrbitalParentJob2 : IJobEntity {
            public OrbitalDatabaseComponent database;
            public EntityCommandBuffer ecb;
            
            public void Execute(Entity entity, ref AddOrbitalParent parent) {
                ecb.AddSharedComponent<OrbitalParent>(entity, new OrbitalParent {
                        Value = database.LookupEntity(parent.Value),
                        Name = parent.Value,
                    });
                ecb.AddComponent<OrbitalParentPosition>(entity, new OrbitalParentPosition {
                        Value = double3.zero
                    });
                ecb.RemoveComponent<AddOrbitalParent>(entity);
            }
        }

        public static void AddOrbitalParent(
            in Entity entity, in FixedString64Bytes name, in OrbitalDatabaseComponent database,
            ref EntityCommandBuffer ecb)
        {
            var data = database.LookupData(name);
            // Debug.Log($"looking up {name} ({entity}) -> {data.Parent} == ({database.EntityMap[data.Parent]})");

            ecb.AddSharedComponent<OrbitalParent>(entity, new OrbitalParent {
                    Value = database.LookupEntity(data.Parent),
                    Name = data.Parent,
                });
            ecb.AddComponent<OrbitalParentPosition>(entity, new OrbitalParentPosition {
                    Value = double3.zero
                });
        }

        public static void AddOrbitalParameters(
            in Entity entity, in FixedString64Bytes name,
            ref OrbitalDatabaseComponent database, ref EntityCommandBuffer ecb)
        {
            var data = database.LookupData(name);
            var player = data.Type == "Player";
                    
            // set orbital tag
            if (data.Type == "Planet") ecb.AddComponent<PlanetTag>(entity);
            else if (data.Type == "Moon") ecb.AddComponent<MoonTag>(entity);
            else if (data.Type == "DwarfPlanet") ecb.AddComponent<DwarfPlanetTag>(entity);
            else if (data.Type == "Asteroid") ecb.AddComponent<AsteroidTag>(entity);
            else if (data.Type == "Ship") ecb.AddComponent<ShipTag>(entity);
            else if (data.Type == "Player") ecb.AddComponent<ShipTag>(entity);
            else throw new System.Exception("invalid orbital body type: " + data.Type);

            // special sun tag
            if (data.Name == "Sun") ecb.AddComponent<SunTag>(entity);
                    
            // add orbital parameters
            ecb.AddComponent<OrbitalParameters>(entity, new OrbitalParameters {
                    Period = data.Period,
                    Eccentricity = data.Eccentricity,
                    SemiMajorAxis = data.SemiMajorAxis,
                    Inclination = data.Inclination,
                    AscendingNode = data.AscendingNode,
                    // XXX shouldn't we have an argument of periapsis?
                    OrbitRotation = dquaternion.EulerYXZ(math.radians(data.Inclination),
                                                         math.radians(data.AscendingNode),
                                                         0)
                });
                    
            // add orbital position
            ecb.AddComponent<OrbitalPosition>(entity, new OrbitalPosition {
                    ElapsedTime = data.ElapsedTime,
                    LocalToParent = double3.zero,
                    LocalToWorld = double3.zero
                });
                    
            // skip rotational and scale components on the player
            if (!player) {
                // add orbital rotation
                ecb.AddComponent<RotationalParameters>(entity, new RotationalParameters {
                        Tilt = data.AxialTilt,
                        NorthPoleRA = data.NorthPoleRA,
                        Period = data.RotationPeriod,
                        ElapsedTime = data.RotationElapsedTime,
                        AxialRotation = quaternion.EulerXYZ(0f),
                        AxialTilt = math.mul(quaternion.RotateX(-math.radians(data.AxialTilt)),
                                             quaternion.RotateY(-math.radians(data.NorthPoleRA)))
                    });
                        
                // add orbital scale
                ecb.AddComponent<OrbitalScale>(entity, new OrbitalScale {
                        Radius = data.Radius
                    });
                
                // assume we don't want to render this body (unless a planet)
                if (data.Type == "Planet") {
                    ecb.AddComponent<OrbitRenderingEnabled>(entity);
                } else {
                    ecb.AddComponent<OrbitRenderingDisabled>(entity);
                }
            }

            // add special sunlight component on the sun
            if (data.Name == "Sun") {
                ecb.AddComponent<SunLightComponent>(entity);
            }

            // add to database
            database.SaveEntity(name, entity);
            // Debug.Log($"added {data} to {entity}");
        }
    }
}
