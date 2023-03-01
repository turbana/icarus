using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

namespace Icarus.UI {
    /* An Interaction represents the user pressing a key */
    public partial struct Interaction : IComponentData {
        public bool LeftClick;     // left click
        public bool LeftClickDown; // holding left mouse
        public bool ScrollUp;      // scroll wheel up
        public bool ScrollDown;    // scroll wheel down
        public bool GiveControl;   // E key

        public bool AnyInteraction { get => (LeftClick || LeftClickDown || ScrollUp || ScrollDown || GiveControl) ;}
    }

    /* InteractionControlType is used to signify a desired increase or decrease
     * in a control value. */
    public enum InteractionControlType { Increase, Decrease, Toggle };

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
