using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

using Icarus.Misc;

namespace Icarus.Orbit {
    public struct OrbitalOptions : IComponentData {
        public float TimeScale;
    }

    [AddComponentMenu("Icarus/Orbits/Orbital Options")]
    public class OrbitalOptionsAuthoring : MonoBehaviour {
        public float TimeScale;
        
        public class Baker : Unity.Entities.Baker<OrbitalOptionsAuthoring> {
            public override void Bake(OrbitalOptionsAuthoring parms) {
                AddComponent(new OrbitalOptions {
                        TimeScale = parms.TimeScale
                    });
            }
        }
    }
}
