using UnityEngine;
using Unity.Entities;
using Unity.Transforms;

namespace Icarus.Orbit {
    public struct OrbitalParent : IComponentData {
        public Entity Value;
        public UniformScaleTransform ParentToWorld;
    }

    [AddComponentMenu("Icarus/Orbits/Orbital Parent")]
    public class OrbitalParentAuthoring : MonoBehaviour {
        public OrbitalParametersAuthoring ParentBody;

        public class Baker : Unity.Entities.Baker<OrbitalParentAuthoring> {
            public override void Bake(OrbitalParentAuthoring obj) {
                AddComponent(new OrbitalParent {
                        Value = GetEntity(obj.ParentBody.gameObject),
                        ParentToWorld = new UniformScaleTransform {Scale = 1f}
                    });
            }
        }
    }
}
