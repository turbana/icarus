using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Icarus.Orbit {
    public struct OrbitalBodyToLoadComponent : IComponentData {
        public FixedString64Bytes Name;
    }

    [AddComponentMenu("Icarus/Orbit/Orbital Body To Load Component")]
    public class OrbitalBodyToLoadAuthoring : MonoBehaviour {
        public string Name;
        
        public class OrbitalBodyToLoadAuthoringBaker : Baker<OrbitalBodyToLoadAuthoring> {
            public override void Bake(OrbitalBodyToLoadAuthoring auth) {
                AddComponent(new OrbitalBodyToLoadComponent {
                        Name = auth.Name
                    });
            }
        }
    }
}
