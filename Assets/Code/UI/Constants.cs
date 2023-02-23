using Unity.Mathematics;

namespace Icarus.UI {
    public static class Constants {
        // interactions raycast travel this far
        public const float INTERACT_DISTANCE = 2f; // in meters

        // physics layer mask for interactions
        public const uint INTERACTION_LAYER_MASK = 1u << 3;
        
        // rotate TwoWayControls by this radians
        public const float TWO_WAY_ROTATE_ANGLE = (80f / 360f) * 2f * math.PI;
    }
}
