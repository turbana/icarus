using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Icarus.UI {
    /* InteractionType is an index into the bit field Interaction.Mask */
    public enum InteractionType : uint {
        LeftMouse = 0,          // state of left mouse button
        LeftMouseDown,          // 1 on the frame left mouse button is first pressed
        LeftMouseUp,            // 1 on the frame left mouse button is released
        ScrollWheelUp,          // 1 on the frame mouse scroll wheel is up
        ScrollWheelDown,        // 1 on the frame mouse scroll wheel is down
        GiveControl,            // 1 on the frame the main interaction key is pressed
    }

    /* An Interaction represents a set of Interactions (either user
     * generated or a set a collider is willing to consume) */
    public partial struct Interaction : IComponentData {
        public BitField32 Value; // what interactions are currently occurring?
        public BitField32 Mask;  // what interactions will this consume

        public bool AnyInteraction { get => Value.Value != 0u; }
        public bool LeftMouse { get => Value.IsSet((int)InteractionType.LeftMouse); }
        public bool LeftMouseDown { get => Value.IsSet((int)InteractionType.LeftMouseDown); }
        public bool LeftMouseUp { get => Value.IsSet((int)InteractionType.LeftMouseUp); }
        public bool ScrollWheelUp { get => Value.IsSet((int)InteractionType.ScrollWheelUp); }
        public bool ScrollWheelDown { get => Value.IsSet((int)InteractionType.ScrollWheelDown); }
        public bool GiveControl { get => Value.IsSet((int)InteractionType.GiveControl); }

        public bool CanConsume(Interaction inputs) {
            return 0 != (Mask.Value & inputs.Value.Value);
        }

        public static Interaction FromUserInput() {
            var scroll = Input.mouseScrollDelta[1];
            var mask = (Input.GetMouseButton(0) ? 1<<(int)InteractionType.LeftMouse : 0)
                | (Input.GetMouseButtonDown(0) ? 1<<(int)InteractionType.LeftMouseDown : 0)
                | (Input.GetMouseButtonUp(0) ? 1<<(int)InteractionType.LeftMouseUp : 0)
                | (scroll > 0f ? 1<<(int)InteractionType.ScrollWheelUp : 0)
                | (scroll < 0f ? 1<<(int)InteractionType.ScrollWheelDown : 0)
                | (Input.GetKeyDown("e") ? 1<<(int)InteractionType.GiveControl : 0);
            return new Interaction() {
                Value = new BitField32() { Value = (uint)mask },
                Mask = default,
            };
        }

        public static Interaction FromMask(params InteractionType[] types) {
            int mask = 0;
            foreach (var type in types) {
                mask |= 1 << (int)type;
            }
            return new Interaction() {
                Value = default,
                Mask = new BitField32() { Value = (uint)mask },
            };
        }
    }

    /* InteractionControlType is used to signify a desired increase or decrease
     * in a control value. */
    public enum InteractionControlType { Increase, Decrease, Toggle, Press };

    /* An InteractionControl is attached to a physics collider and is used to
     * configure the desired interaction. */
    public partial struct InteractionControl : IComponentData {
        public Entity Control;
        public InteractionControlType Type;
    }

    /* A ControlValue is the state of a control (switch, dial, etc) */
    public partial struct ControlValue : IComponentData {
        public int Value;
        public int PreviousValue;
    }

    /* ControlSettings contain various setting for a control. */
    public partial struct ControlSettings : IComponentData {
        public int Stops;       // number of "stops" on the dial/switch
        public float Rotation;  // amount of rotation for each stop (in radians)
        public float3 Movement; // amount of movement for each stop
    }

    /* UpdateInteractionSystemGroup is where Interactions are performed */
    public partial class UpdateInteractionSystemGroup : ComponentSystemGroup {}

    /* All Control*Authoring classes should inherit from this. Makes selecting
     * GameObjects from the editor easier. */
    public class BaseControlAuthoring : MonoBehaviour { }

    /* Cross hair types */
    public enum CrosshairType : int {
        Normal = 0, Toggle, Increase, Decrease, Enter
    }

    /* Crosshair Component */
    public class Crosshair : IComponentData {
        public CrosshairType Value;
        public GameObject GO;
        public Sprite[] Crosshairs;
    }
}
