using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Icarus.UI {
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(UpdateInteractionSystemGroup))]
    public partial class MultiWayControlSystem : SystemBase {
        private enum ControlType { None, Increase, Decrease };
        public ComponentLookup<ControlValue> ControlValueLookup;
        public ComponentLookup<LocalTransform> LocalTransformLookup;
        public ComponentLookup<ControlSettings> ControlSettingsLookup;

        [BurstCompile]
        protected override void OnCreate() {
            ControlValueLookup = GetComponentLookup<ControlValue>(false);
            LocalTransformLookup = GetComponentLookup<LocalTransform>(false);
            ControlSettingsLookup = GetComponentLookup<ControlSettings>(true);
        }
        
        [BurstCompile]
        protected override void OnUpdate() {
            ControlValueLookup.Update(this);
            LocalTransformLookup.Update(this);
            ControlSettingsLookup.Update(this);
            
            new MultiWayControlJob {
                ControlValueLookup = ControlValueLookup,
                LocalTransformLookup = LocalTransformLookup,
                ControlSettingsLookup = ControlSettingsLookup,
            }.Schedule();
        }

        // [BurstCompile]
        [WithChangeFilter(typeof(Interaction))]
        protected partial struct MultiWayControlJob : IJobEntity {
            public ComponentLookup<ControlValue> ControlValueLookup;
            public ComponentLookup<LocalTransform> LocalTransformLookup;
            [ReadOnly]
            public ComponentLookup<ControlSettings> ControlSettingsLookup;
            
            // [BurstCompile]
            public void Execute(Entity entity, ref Interaction interaction, in InteractionControl control) {
                // UnityEngine.Debug.Log("update control");
                var direction = 0;

                // check desired interactions
                if (interaction.ScrollUp || (interaction.LeftClick && control.Type == InteractionControlType.Increase)) {
                    direction = 1;
                    // UnityEngine.Debug.Log("direction increase");
                } else if (interaction.ScrollDown || (interaction.LeftClick && control.Type == InteractionControlType.Decrease)) {
                    direction = -1;
                    // UnityEngine.Debug.Log("direction decrease");
                }

                // are we updating?
                if (direction != 0) {
                    var parent = control.Control;
                    var value = ControlValueLookup[parent];
                    var settings = ControlSettingsLookup[parent];
                    var next = value.Value + direction;
                    // is the next value in range?
                    if (0 <= next && next < settings.Stops) {
                        // UnityEngine.Debug.Log($"next in range {next} ({settings.Stops})");
                        var rotate = quaternion.RotateX(settings.RotateAngle * direction);
                        // update main entity rotation
                        var plt = LocalTransformLookup[parent];
                        plt.Rotation = math.mul(plt.Rotation, rotate);
                        LocalTransformLookup[parent] = plt;
                        // update with new control value
                        ControlValueLookup[parent] = new ControlValue {
                            Value = next,
                            PreviousValue = value.Value,
                        };
                    }
                    // clear collider
                    interaction = new Interaction();
                }
            }
        }
    }
}
