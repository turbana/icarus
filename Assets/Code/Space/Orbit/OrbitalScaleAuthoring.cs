using UnityEngine;
using Unity.Entities;

namespace Icarus.Orbit {
    public struct OrbitalScale : IComponentData {
        public float Radius;
    }

    [AddComponentMenu("Icarus/Orbits/Orbital Scale")]
    public class OrbitalScaleAuthoring : MonoBehaviour {
        public float Radius;
        
        public class Baker : Unity.Entities.Baker<OrbitalScaleAuthoring> {
            public override void Bake(OrbitalScaleAuthoring parms) {
                AddComponent(new OrbitalScale {
                        Radius = parms.Radius
                    });
            }
        }
    }
}
