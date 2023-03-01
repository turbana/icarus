using UnityEngine;
using Unity.Collections;
using Unity.Entities;

namespace Icarus.UI {
    public class ControlColliderAuthoring : MonoBehaviour {
        [Tooltip("The control for this collider")]
        public BaseControlAuthoring Control;
        [Tooltip("The type of this collider")]
        public InteractionControlType Type;
        
        public class ControlColliderAuthoringBaker : Baker<ControlColliderAuthoring> {
            public override void Bake(ControlColliderAuthoring auth) {
                Interaction interaction = default;
                switch (auth.Type) {
                    case InteractionControlType.Increase:
                    case InteractionControlType.Decrease:
                        interaction = Interaction.FromMask(
                            InteractionType.LeftMouseDown,
                            InteractionType.ScrollWheelUp,
                            InteractionType.ScrollWheelDown);
                            break;
                    case InteractionControlType.Toggle:
                    case InteractionControlType.Press:
                        interaction = Interaction.FromMask(
                            InteractionType.LeftMouseDown);
                        break;
                }
                AddComponent<Interaction>(interaction);
                AddComponent<InteractionControl>(new InteractionControl {
                        Control = GetEntity(auth.Control.gameObject),
                        Type = auth.Type,
                    });
            }
        }
    }
}
