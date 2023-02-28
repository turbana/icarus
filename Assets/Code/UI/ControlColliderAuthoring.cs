using UnityEngine;
using Unity.Entities;

namespace Icarus.UI {
    public class ControlColliderAuthoring : MonoBehaviour {
        [Tooltip("The control for this collider")]
        public BaseControlAuthoring Control;
        [Tooltip("The type of this collider")]
        public InteractionControlType Type;
        
        public class ControlColliderAuthoringBaker : Baker<ControlColliderAuthoring> {
            public override void Bake(ControlColliderAuthoring auth) {
                AddComponent<Interaction>();
                AddComponent<InteractionControl>(new InteractionControl {
                        Control = GetEntity(auth.Control.gameObject),
                        Type = auth.Type,
                    });
            }
        }
    }
}
