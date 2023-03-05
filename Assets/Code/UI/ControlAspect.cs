using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Icarus.UI {
    public readonly partial struct ControlAspect : IAspect {
        readonly RefRO<ControlSettings> Settings;
        readonly RefRO<DatumRef> DatumRef;

        public Entity Datum => DatumRef.ValueRO.Entity;
        public InteractionControlType ControlType => Settings.ValueRO.Type;
        public int StopCount => Settings.ValueRO.Stops;
        public Entity Root => Settings.ValueRO.Root;

        public byte NextValue(in DatumByte datum, in Interaction inputs) {
            int direction = 0;

            // check desired interactions
            if (inputs.ScrollWheelUp || (inputs.LeftMouseDown && ControlType == InteractionControlType.Increase)) {
                direction = 1;
                // UnityEngine.Debug.Log("direction increase");
            } else if (inputs.ScrollWheelDown || (inputs.LeftMouseDown && ControlType == InteractionControlType.Decrease)) {
                direction = -1;
                // UnityEngine.Debug.Log("direction decrease");
            } else if (inputs.LeftMouseDown && ControlType == InteractionControlType.Toggle) {
                direction = datum.PreviousValue - datum.Value;
                if (direction == 0) {
                    if (datum.Value == 0) {
                        direction = 1;
                    } else if (0 < datum.Value) {
                        direction = -1;
                    } else {
                        UnityEngine.Debug.LogError($"datum value cannot be negative: {datum.Value}");
                    }
                }
                // UnityEngine.Debug.Log($"direction toggled to {direction}");
            } else if (ControlType == InteractionControlType.Press) {
                direction = (inputs.LeftMouse ? 1 : -1);
                // UnityEngine.Debug.Log($"direction pressed to {direction}");
            }

            if (direction != 0) {
                var next = datum.Value + direction;
                if (0 <= next && next < StopCount) {
                    return (byte)next;
                }
            }

            return datum.Value;
        }

        public CrosshairType Crosshair(in DatumByte datum) {
            switch (ControlType) {
                case InteractionControlType.Increase:
                    return CrosshairType.Increase;
                case InteractionControlType.Decrease:
                    return CrosshairType.Decrease;
                case InteractionControlType.Toggle:
                case InteractionControlType.Press:
                    return CrosshairType.Toggle;
            }
            return CrosshairType.Normal;
        }

        public LocalTransform GetLocalTransform(in DatumByte datum) {
            var translation = Settings.ValueRO.Movement * datum.Value;
            var rotation = quaternion.RotateX(Settings.ValueRO.Rotation * datum.Value);
            var transform = Settings.ValueRO.InitialTransform;
            transform.Position = transform.Position + translation;
            transform.Rotation = math.mul(transform.Rotation, rotation);
            return transform;
        }
    }
}
