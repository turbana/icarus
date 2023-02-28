using UnityEngine;
using Unity.Entities;

namespace Icarus.Misc {
    public class ReParentAuthoring : MonoBehaviour {
        public class ReParentAuthoringBaker : Baker<ReParentAuthoring> {
            public override void Bake(ReParentAuthoring auth) {
                var parent = auth.gameObject.transform.parent;
                DependsOn(parent);
                AddComponent<ReParent>(new ReParent {
                        Value = GetEntity(parent),
                    });
            }
        }
    }
}
