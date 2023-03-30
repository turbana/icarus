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
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new DebugSatellites {
                        Prefab = GetEntity(auth.prefab, TransformUsageFlags.Dynamic)
                    });
            }
        }
    }
}
