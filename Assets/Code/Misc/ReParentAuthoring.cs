using UnityEngine;
using Unity.Entities;

namespace Icarus.Misc {
    public class ReParentAuthoring : MonoBehaviour {
        public class ReParentAuthoringBaker : Baker<ReParentAuthoring> {
            public override void Bake(ReParentAuthoring auth) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                var parent = auth.gameObject.transform.parent;
                DependsOn(parent);
                AddComponent<ReParent>(entity, new ReParent {
                        Value = GetEntity(parent, TransformUsageFlags.Dynamic),
                    });
            }
        }
    }
}
