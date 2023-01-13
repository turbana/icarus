using UnityEngine;
using Unity.Entities;
using Unity.Transforms;

namespace Icarus.Orbit {
    public struct OrbitalParent : ISharedComponentData {
        public Entity Value;
    }

    public struct OrbitalParentPosition : IComponentData {
        public LocalTransform Value;
    }

    [AddComponentMenu("Icarus/Orbits/Orbital Parent")]
    public class OrbitalParentAuthoring : MonoBehaviour {
        public OrbitalParametersAuthoring ParentBody;

        public class Baker : Unity.Entities.Baker<OrbitalParentAuthoring> {
            public override void Bake(OrbitalParentAuthoring obj) {
                AddSharedComponent(new OrbitalParent {
                        Value = GetEntity(obj.ParentBody.gameObject)
                    });
                AddComponent(new OrbitalParentPosition {
                        Value = LocalTransform.FromScale(1f)
                    });
            }
        }
    }
}
