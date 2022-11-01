using UnityEngine;
using Unity.Entities;

namespace Icarus.Orbit {
    public struct OrbitalParent : IComponentData {
        public Entity Value;
    }

    [AddComponentMenu("Icarus/Orbits/Orbital Parent")]
    public class OrbitalParentAuthoring : MonoBehaviour {
        public OrbitalParametersAuthoring ParentBody;

        public class Baker : Unity.Entities.Baker<OrbitalParentAuthoring> {
            public override void Bake(OrbitalParentAuthoring obj) {
                AddComponent(new OrbitalParent {
                        Value = GetEntity(obj.ParentBody.gameObject)
                    });
            }
        }
    }
}
