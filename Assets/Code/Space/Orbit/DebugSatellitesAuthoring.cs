using UnityEngine;
using Unity.Entities;

namespace Icarus.Orbit {
    public struct DebugSatellites : IComponentData {
        public Entity Prefab;
    }
    
    [AddComponentMenu("Icarus/Debug/Spawn Satellites")]
    public class DebugSatellitesAuthoring : MonoBehaviour {
        public GameObject prefab;
        
        public class Baker : Unity.Entities.Baker<DebugSatellitesAuthoring> {
            public override void Bake(DebugSatellitesAuthoring auth) {
                AddComponent(new DebugSatellites {
                        Prefab = GetEntity(auth.prefab)
                    });
            }
        }
    }
}
