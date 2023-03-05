using UnityEngine;

namespace Icarus.UI {
    public class SimpleButtonAuthoring : BaseControlAuthoring {
        [Tooltip("Is this a toggle button (otherwise a push button)?")]
        public bool IsToggle;
        [Tooltip("How much movement should occur with a press?")]
        public Vector3 Movement;
    }
}
