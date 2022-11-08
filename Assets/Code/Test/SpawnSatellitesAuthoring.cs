using Unity.Entities;
using UnityEngine;

namespace Icarus.Orbit {
    public struct SpawnSatellitesComponent : IComponentData {
        public int Count;
        public Entity Prefab;
    }

    [AddComponentMenu("Icarus/Debug/Spawn Satellites")]
    public class SpawnSatellitesAuthoring : MonoBehaviour {
        public int count;
        public GameObject prefab;

        public class Baker : Unity.Entities.Baker<SpawnSatellitesAuthoring> {
            public override void Bake(SpawnSatellitesAuthoring auth) {
                AddComponent(new SpawnSatellitesComponent {
                        Count = auth.count,
                        Prefab = GetEntity(auth.prefab)
                    });
            }
        }
    }
}
