using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

using Icarus.Orbit;

namespace Icarus.Test {
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(UpdateOrbitSystemGroup))]
    [UpdateBefore(typeof(UpdateOrbitalPositionSystem))]
    public partial class SpawnSatellitesSystem : SystemBase {
        protected override void OnUpdate() {
            if (!UnityEngine.Input.GetKeyDown("q")) return;
            Entity player = GetSingletonEntity<PlayerOrbitTag>();
            OrbitalParameters parms = GetComponent<OrbitalParameters>(player);
            OrbitalPosition pos = GetComponent<OrbitalPosition>(player);
            OrbitalParent parent = GetComponent<OrbitalParent>(player);
            SpawnSatellitesComponent spawn =
                GetComponent<SpawnSatellitesComponent>(
                    GetSingletonEntity<SpawnSatellitesComponent>());
            OrbitalScale scale = new OrbitalScale { Radius = 0.001f };
                        
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
            NativeArray<Entity> entities = new NativeArray<Entity>(spawn.Count, Allocator.TempJob);
            var rand = new Random((uint)System.Diagnostics.Stopwatch.GetTimestamp());

            UnityEngine.Debug.Log(".");
            ecb.Instantiate(spawn.Prefab, entities);
            ecb.AddComponent<OrbitalParent>(entities, parent);
            ecb.AddComponent<OrbitalScale>(entities, scale);
            ecb.AddComponent<ShipTag>(entities);
            for (int i=0; i<spawn.Count; i++) {
                float period = parms.Period + rand.NextFloat(-30f, 30f);
                float elapsed = pos.ElapsedTime + rand.NextFloat(-30f, 30f);
                if (elapsed < 0f) elapsed += period;
                ecb.AddComponent<OrbitalParameters>(entities[i], new OrbitalParameters {
                        Eccentricity = parms.Eccentricity + rand.NextFloat(-0.01f, 0.01f),
                        SemiMajorAxis = parms.SemiMajorAxis + rand.NextFloat(-100f, 100f),
                        Inclination = parms.Inclination + rand.NextFloat(-1f, 1f),
                        AscendingNode = parms.AscendingNode + rand.NextFloat(-1f, 1f),
                        Period = period
                    });
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
    }
}
