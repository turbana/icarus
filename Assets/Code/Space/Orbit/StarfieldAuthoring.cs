using UnityEngine;
using Unity.Entities;

namespace Icarus.Orbit {
    public struct StarfieldTag : IComponentData {}

    [AddComponentMenu("Icarus/Orbits/Starfield")]
    public class StarfieldAuthoring : MonoBehaviour {
        public class Baker : Unity.Entities.Baker<StarfieldAuthoring> {
            public override void Bake(StarfieldAuthoring parms) {
                AddComponent(new StarfieldTag());
            }
        }
    }
}
