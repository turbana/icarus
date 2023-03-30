using UnityEngine;
using Unity.Entities;

namespace Icarus.Controls {
    public enum ControlSystemType {
        BridgeJumpTargetKeypad,
        BridgeJumpTargetKeyboard,
        BridgeJumpTargetLoad,
        BridgeJumpTargetJump,
        DebugTimeControls,
        DebugSpawnObjects,
    }
    
    public class ControlSystemAuthoring : MonoBehaviour {
        public ControlSystemType ControlSystem;

        public class ControlSystemAuthoringBaker : Baker<ControlSystemAuthoring> {
            public override void Bake(ControlSystemAuthoring auth) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                switch (auth.ControlSystem) {
                    case ControlSystemType.BridgeJumpTargetKeypad:
                        AddComponent<BridgeJumpTargetKeypadTag>(entity); break;
                    case ControlSystemType.BridgeJumpTargetKeyboard:
                        var buffer = AddBuffer<BridgeJumpTargetValue>(entity);
                        buffer.Length = 10;
                        for (int i=0; i<10; i++) {
                            buffer[i] = new BridgeJumpTargetValue { Value = "" };
                        }
                        AddComponent<BridgeJumpTargetKeyboardTag>(entity); break;
                    case ControlSystemType.BridgeJumpTargetLoad:
                        AddComponent<BridgeJumpTargetLoadTag>(entity); break;
                    case ControlSystemType.BridgeJumpTargetJump:
                        AddComponent<BridgeJumpTargetJumpTag>(entity); break;
                    case ControlSystemType.DebugTimeControls:
                        AddComponent<DebugTimeControlsTag>(entity); break;
                    case ControlSystemType.DebugSpawnObjects:
                        AddComponent<DebugSpawnObjectsTag>(entity); break;
                }
            }
        }
    }
}
