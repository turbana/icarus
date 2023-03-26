using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

using Icarus.Mathematics;
using Icarus.Orbit;
using Icarus.UI;

namespace Icarus.Controls {
    public partial struct DebugSpawnObjectsTag : IComponentData {}

    [BurstCompile]
    [UpdateInGroup(typeof(IcarusInteractionSystemGroup))]
    public partial class DebugSpawnObjectsSystem : SystemBase {
        public ComponentLookup<DatumDouble> DatumDoubleLookup;
        private Random random;

        private const double INITIAL_COUNT = 100;

        [BurstCompile]
        protected override void OnCreate() {
            DatumDoubleLookup = GetComponentLookup<DatumDouble>(false);
            random = new Random((uint)System.Diagnostics.Stopwatch.GetTimestamp());
        }

        [BurstCompile]
        protected override void OnUpdate() {
            DatumDoubleLookup.Update(this);
            var DDL = DatumDoubleLookup;
            var rand = random;

            Entity player = SystemAPI.GetSingletonEntity<PlayerOrbitTag>();
            OrbitalParameters parms = SystemAPI.GetComponent<OrbitalParameters>(player);
            OrbitalPosition pos = SystemAPI.GetComponent<OrbitalPosition>(player);
            OrbitalParent parent = this.EntityManager.GetSharedComponent<OrbitalParent>(player);
            OrbitalParentPosition ppos = SystemAPI.GetComponent<OrbitalParentPosition>(player);
            Entity prefab = SystemAPI.GetSingleton<SpawnSatellitesComponent>().Prefab;
            
            NativeList<Entity> entitiesList = new NativeList<Entity>(10, Allocator.TempJob);
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
        
            Entities
                .WithAll<DebugSpawnObjectsTag>()
                .ForEach((in DatumRefBufferCollection index,
                          in DynamicBuffer<DatumRefBuffer> buffers) => {
                    var increase = DDL[buffers[index["Debug.SpawnObjects.Increase"]].Entity];
                    var decrease = DDL[buffers[index["Debug.SpawnObjects.Decrease"]].Entity];
                    var spawn = DDL[buffers[index["Debug.SpawnObjects.Spawn"]].Entity];
                    var countEntity = buffers[index["Debug.SpawnObjects.Count"]].Entity;
                    var countDatum = DDL[countEntity];

                    if (increase.Dirty && increase.Value == 1) {
                        countDatum.Value = countDatum.Value * 10;
                    } else if (decrease.Dirty && decrease.Value == 1) {
                        countDatum.Value = countDatum.Value / 10;
                    } else if (spawn.Dirty && spawn.Value == 1) {
                        OrbitalScale scale = new OrbitalScale { Radius = 0.001f };
                        RotationalParameters rot = new RotationalParameters {
                            Tilt = 0f,
                            NorthPoleRA = 0f,
                            Period = 1f,
                            ElapsedTime = 0f,
                            AxialTilt = quaternion.EulerXYZ(0f),
                            AxialRotation = quaternion.EulerXYZ(0f)
                        };

                        entitiesList.Length = (int)countDatum.Value;
                        var entities = entitiesList.AsArray();
                        ecb.Instantiate(prefab, entities);
                        ecb.AddSharedComponent<OrbitalParent>(entities, parent);
                        ecb.AddComponent<OrbitalParentPosition>(entities, ppos);
                        ecb.AddComponent<OrbitalScale>(entities, scale);
                        ecb.AddComponent<RotationalParameters>(entities, rot);
                        ecb.AddComponent<ShipTag>(entities);
                        ecb.AddComponent<OrbitRenderingEnabled>(entities);
                        ecb.AddComponent<PlayerSiblingOrbitTag>(entities);
                        for (int i=0; i<countDatum.Value; i++) {
                            double period = parms.Period + rand.NextDouble(0f, 0f);
                            double elapsed = pos.ElapsedTime + rand.NextDouble(-0.1f, 0.1f);
                            if (elapsed < 0f) elapsed += period;
                            var nparms = new OrbitalParameters {
                                Period = period,
                                Eccentricity = parms.Eccentricity + rand.NextDouble(-0.0000001f, 0.0000001f),
                                SemiMajorAxis = parms.SemiMajorAxis + rand.NextDouble(-0.1f, 0.1f),
                                Inclination = parms.Inclination + rand.NextDouble(-0.001f, 0.001f),
                                AscendingNode = parms.AscendingNode + rand.NextDouble(0f, 0f)
                            };
                            nparms.OrbitRotation = dquaternion
                                .EulerYXZ(math.radians(nparms.Inclination),
                                          math.radians(nparms.AscendingNode),
                                          0f);
                            ecb.AddComponent<OrbitalParameters>(entities[i], nparms);
                            ecb.AddComponent<OrbitalPosition>(entities[i], new OrbitalPosition {
                                    ElapsedTime = elapsed,
                                    Theta = pos.Theta,
                                    Altitude = pos.Altitude,
                                    LocalToWorld = pos.LocalToWorld,
                                    LocalToParent = pos.LocalToParent
                                });
                        }
                    }
                    if (countDatum.Dirty) {
                        if (countDatum.Value == 0.0) {
                            countDatum.Value = INITIAL_COUNT;
                        }
                        DDL[countEntity] = countDatum;
                    }
                })
                .Schedule();

            this.Dependency.Complete();
            ecb.Playback(this.EntityManager);
            ecb.Dispose();
            entitiesList.Dispose();
        }
    }
}
