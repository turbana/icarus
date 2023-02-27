using UnityEngine;
using Unity.Entities;
// using Unity.Physics;

namespace Icarus.UI {
    public class MultiWayColliderAuthoring : MonoBehaviour {
        [Tooltip("The control for this collider")]
        public MultiWayControlAuthoring Control;
        [Tooltip("The type of this collider")]
        public InteractionControlType Type;
        
        public class MultiWayColliderAuthoringBaker : Baker<MultiWayColliderAuthoring> {
            public override void Bake(MultiWayColliderAuthoring auth) {
                AddComponent<Interaction>();
                AddComponent<InteractionControl>(new InteractionControl {
                        Control = GetEntity(auth.Control.gameObject),
                        Type = auth.Type,
                    });
            }
        }
    }
}
