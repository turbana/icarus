using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Icarus.UI {
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(UpdateInteractionSystemGroup))]
    [BurstCompile]
    public partial class TwoWayControlSystem : SystemBase {
        [BurstCompile]
        protected override void OnUpdate() {
            Entities
                .WithChangeFilter<Interaction>()
                .ForEach((Entity entity, ref Interaction interaction, ref ControlValue control, ref LocalTransform transform) =>
                {
                    // update control's value
                    if (interaction.Toggle) {
                        control.PreviousValue = control.Value;
                        control.Value = (control.Value == 0f) ? 1f : 0f;
                        // UnityEngine.Debug.Log($"Toggle: [{control.Value} | {control.PreviousValue}]");
                    } else if (interaction.ScrollUp) {
                        control.PreviousValue = control.Value;
                        control.Value = 1f;
                        // UnityEngine.Debug.Log($"ScrollUp: [{control.Value} | {control.PreviousValue}]");
                    } else if (interaction.ScrollDown) {
                        control.PreviousValue = control.Value;
                        control.Value = 0f;
                        // UnityEngine.Debug.Log($"ScrollDown: [{control.Value} | {control.PreviousValue}]");
                    } else {
                        // our Interaction wasn't actually updated
                        return;
                    }

                    // move geometry
                    if (control.Value != control.PreviousValue) {
                        var angle = (control.Value == 0f)
                            ? -Constants.TWO_WAY_ROTATE_ANGLE
                            : Constants.TWO_WAY_ROTATE_ANGLE;
                        // UnityEngine.Debug.Log($"rotating: {angle}");
                        transform = transform.RotateX(angle);
                    }
                    
                    // clear interaction
                    interaction = new Interaction();
                    })
                .Schedule();
        }
    }
}
