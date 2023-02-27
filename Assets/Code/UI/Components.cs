using Unity.Entities;

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
    public enum InteractionControlType { Increase, Decrease };

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
        public int Stops;         // number of "stops" on the dial/switch
        public float RotateAngle; // travel of each stop on the switch (in radians)
    }

    /* UpdateInteractionSystemGroup is where Interactions are performed */
    public partial class UpdateInteractionSystemGroup : ComponentSystemGroup {}
}
