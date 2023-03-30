using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Icarus.Orbit {
    public struct OrbitalParent : ISharedComponentData {
        public Entity Value;
        public FixedString64Bytes Name;
    }

    public struct OrbitalParentPosition : IComponentData {
        public double3 Value;
    }

    [AddComponentMenu("Icarus/Orbits/Orbital Parent")]
    public class OrbitalParentAuthoring : MonoBehaviour {
        public OrbitalParametersAuthoring ParentBody;

        public class Baker : Unity.Entities.Baker<OrbitalParentAuthoring> {
            public override void Bake(OrbitalParentAuthoring obj) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddSharedComponent(entity, new OrbitalParent {
                        Value = GetEntity(obj.ParentBody.gameObject, TransformUsageFlags.Dynamic)
                    });
                AddComponent(entity, new OrbitalParentPosition {
                        Value = double3.zero
                    });
            }
        }
    }
}
