using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Icarus.Orbit {
    public struct PlayerOrbitComponent : IComponentData {
        public FixedString64Bytes Orbit;
    }

    [AddComponentMenu("Icarus/Loading/Player Orbit Component")]
    public class PlayerOrbitAuthoring : MonoBehaviour {
        public string Orbit;
        
        public class PlayerOrbitAuthoringBaker : Baker<PlayerOrbitAuthoring> {
            public override void Bake(PlayerOrbitAuthoring auth) {
                AddComponent<PlayerOrbitTag>();
                AddComponent(new OrbitalBodyToLoadComponent {
                        Name = auth.Orbit
                    });
            }
        }
    }
}
