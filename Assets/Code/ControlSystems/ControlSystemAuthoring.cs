using UnityEngine;
using Unity.Entities;

namespace Icarus.Controls {
    public enum ControlSystemType {
        BridgeJumpTargetKeypad,
        BridgeJumpTargetKeyboard,
        BridgeJumpTargetLoad,
    }
    
    public class ControlSystemAuthoring : MonoBehaviour {
        public ControlSystemType ControlSystem;

        public class ControlSystemAuthoringBaker : Baker<ControlSystemAuthoring> {
            public override void Bake(ControlSystemAuthoring auth) {
                switch (auth.ControlSystem) {
                    case ControlSystemType.BridgeJumpTargetKeypad:
                        AddComponent<BridgeJumpTargetKeypadTag>(); break;
                    case ControlSystemType.BridgeJumpTargetKeyboard:
                        AddComponent<BridgeJumpTargetKeyboardTag>(); break;
                    case ControlSystemType.BridgeJumpTargetLoad:
                        AddComponent<BridgeJumpTargetLoadTag>(); break;
                }
            }
        }
    }
}
