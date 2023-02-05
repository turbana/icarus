using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

using Icarus.Orbit;
using Icarus.Mathematics;

namespace Icarus.Test {
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(UpdateOrbitSystemGroup))]
    [UpdateBefore(typeof(UpdateOrbitalPositionSystem))]
    public partial class SpawnSatellitesSystem : SystemBase {
        private static Random random;
        
        protected override void OnCreate() {
            random = new Random((uint)System.Diagnostics.Stopwatch.GetTimestamp());
        }
        
        protected override void OnUpdate() {
            if (!UnityEngine.Input.GetKeyDown("q")) return;
            Entity player = SystemAPI.GetSingletonEntity<PlayerOrbitTag>();
            OrbitalParameters parms = SystemAPI.GetComponent<OrbitalParameters>(player);
            OrbitalPosition pos = SystemAPI.GetComponent<OrbitalPosition>(player);
            OrbitalParent parent = this.EntityManager.GetSharedComponent<OrbitalParent>(player);
            OrbitalParentPosition ppos = SystemAPI.GetComponent<OrbitalParentPosition>(player);
            SpawnSatellitesComponent spawn =
                SystemAPI.GetComponent<SpawnSatellitesComponent>(
                    SystemAPI.GetSingletonEntity<SpawnSatellitesComponent>());
            OrbitalScale scale = new OrbitalScale { Radius = 0.001f };
            RotationalParameters rot = new RotationalParameters {
                Tilt = 0f,
                NorthPoleRA = 0f,
                Period = 1f,
                ElapsedTime = 0f,
                AxialTilt = quaternion.EulerXYZ(0f),
                AxialRotation = quaternion.EulerXYZ(0f)
            };
                        
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
            NativeArray<Entity> entities = new NativeArray<Entity>(spawn.Count, Allocator.TempJob);

            UnityEngine.Debug.Log(".");
            ecb.Instantiate(spawn.Prefab, entities);
            ecb.AddSharedComponent<OrbitalParent>(entities, parent);
            ecb.AddComponent<OrbitalParentPosition>(entities, ppos);
            ecb.AddComponent<OrbitalScale>(entities, scale);
            ecb.AddComponent<RotationalParameters>(entities, rot);
            ecb.AddComponent<ShipTag>(entities);
            ecb.AddComponent<OrbitRenderingEnabled>(entities);
            ecb.AddComponent<PlayerSiblingOrbitTag>(entities);
            for (int i=0; i<spawn.Count; i++) {
                double period = parms.Period + NextFloat(0f);
                double elapsed = pos.ElapsedTime + NextFloat(0.1f);
                if (elapsed < 0f) elapsed += period;
                var nparms = new OrbitalParameters {
                    Period = period,
                    Eccentricity = parms.Eccentricity + NextFloat(0.0000001f),
                    SemiMajorAxis = parms.SemiMajorAxis + NextFloat(0.1f),
                    Inclination = parms.Inclination + NextFloat(0.001f),
                    AscendingNode = parms.AscendingNode + NextFloat(0f)
                };
                nparms.OrbitRotation = dquaternion.EulerYXZ(math.radians(nparms.Inclination),
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

            ecb.Playback(this.EntityManager);
            ecb.Dispose();
            entities.Dispose();
        }

        private static double NextFloat(double range) {
            return random.NextDouble(-range, range);
        }
    }
}
