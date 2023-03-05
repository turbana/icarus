using UnityEngine;

namespace Icarus.UI {
    public class MultiWayControlAuthoring : BaseControlAuthoring {
        [Tooltip("Number of stops this switch supports")]
        public int Stops = 2;
        [Tooltip("Total degrees of travel this switch moves")]
        public float RotateAngle = 80f;
    }
}
