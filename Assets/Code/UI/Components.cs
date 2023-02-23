using Unity.Entities;

namespace Icarus.UI {
    /* An Interaction represents the user pressing a key */
    public partial struct Interaction : IComponentData {
        public bool Toggle; // left click
        public bool ScrollUp; // scroll wheel up
        public bool ScrollDown; // scroll wheel down
        public bool GiveControl; // E key
    }

    /* A ControlValue is the state of a control (switch, dial, etc) */
    public partial struct ControlValue : IComponentData {
        public float Value;
        public float PreviousValue;
    }

    /* Control type tags */
    public partial struct TwoWayControl : IComponentData {}
    public partial struct ThreeWayControl : IComponentData {}
    public partial struct DialControl : IComponentData {}

    /* UpdateInteractionSystemGroup is where Interactions are performed */
    public partial class UpdateInteractionSystemGroup : ComponentSystemGroup {}
}
