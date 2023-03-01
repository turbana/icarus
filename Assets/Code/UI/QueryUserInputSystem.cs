using UnityEngine;

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace Icarus.UI {
    [BurstCompile]
    public partial class QueryUserInputSystem : SystemBase {
        [ReadOnly]
        private ComponentLookup<ControlValue> ControlValueLookup;
        [ReadOnly]
        private ComponentLookup<ControlSettings> ControlSettingsLookup;
        private ComponentLookup<Interaction> InteractionLookup;
        [ReadOnly]
        private ComponentLookup<InteractionControl> InteractionControlLookup;
        private Camera MainCamera;

        [BurstCompile]
        protected override void OnCreate() {
            ControlValueLookup = GetComponentLookup<ControlValue>(true);
            ControlSettingsLookup = GetComponentLookup<ControlSettings>(true);
            InteractionLookup = GetComponentLookup<Interaction>(false);
            InteractionControlLookup = GetComponentLookup<InteractionControl>(true);
            MainCamera = Camera.main;
        }
        
        protected override void OnUpdate() {
            ControlValueLookup.Update(this);
            ControlSettingsLookup.Update(this);
            InteractionLookup.Update(this);
            InteractionControlLookup.Update(this);
            
            // read user input
            var inputs = Interaction.FromUserInput();
            
            var pworld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            var crosshair = SystemAPI.ManagedAPI.GetSingleton<Crosshair>();
            var next_crosshair = new NativeList<CrosshairType>(1, Allocator.TempJob);
            next_crosshair.Add(CrosshairType.Normal);
            float3 rstart = MainCamera.transform.position;
            float3 rend = rstart + (float3)(MainCamera.transform.forward * Constants.INTERACT_DISTANCE);
            // Debug.Log($"ray casting from {rstart} to {rend} mask={INTERACTION_LAYER_MASK}");
            var CVL = ControlValueLookup;
            var CSL = ControlSettingsLookup;
            var IL = InteractionLookup;
            var ICL = InteractionControlLookup;
            
            Job
                .WithReadOnly(CVL)
                .WithReadOnly(CSL)
                .WithReadOnly(ICL)
                .WithCode(() => {
                Entity entity;
                Raycast(out entity, pworld, rstart, rend);
                // did we hit a collider?
                if (entity != Entity.Null) {
                    // Debug.Log("hit");
                    // Debug.Log($"hit entity={EntityManager.GetName(entity)}");
                    // update collider with inputs
                    var collider = IL[entity];
                    collider.Value = inputs.Value;
                    IL[entity] = collider;
                    // update crosshair
                    var icontrol = ICL[entity];
                    var control = icontrol.Control;
                    var value = CVL[control].Value;
                    var stops = CSL[control].Stops;
                    if (icontrol.Type == InteractionControlType.Increase && (value + 1 < stops)) {
                        next_crosshair[0] = CrosshairType.Increase;
                    } else if (icontrol.Type == InteractionControlType.Decrease && (0 <= value - 1)) {
                        next_crosshair[0] = CrosshairType.Decrease;
                    } else if (icontrol.Type == InteractionControlType.Toggle
                               || icontrol.Type == InteractionControlType.Press) {
                        next_crosshair[0] = CrosshairType.Toggle;
                    } else {
                        next_crosshair[0] = CrosshairType.Normal;
                    }
                } else {
                    // Debug.Log("no hit");
                    next_crosshair[0] = CrosshairType.Normal;
                }
            }).Schedule();
                
            // update crosshair
            this.Dependency.Complete();
            crosshair.Value = next_crosshair[0];
            next_crosshair.Dispose();
        }
        
        [BurstCompile]
        private static void Raycast(out Entity entity, in PhysicsWorldSingleton pworld, in float3 start, in float3 end) {
            var cast = new RaycastInput() {
                Start = start,
                End = end,
                Filter = new CollisionFilter() {
                    BelongsTo = Constants.INTERACTION_LAYER_MASK,
                    CollidesWith = Constants.INTERACTION_LAYER_MASK,
                    GroupIndex = 0
                }
            };
            var hit = new Unity.Physics.RaycastHit();
            if (pworld.CastRay(cast, out hit)) {
                entity = hit.Entity;
            } else {
                entity = Entity.Null;
            }
        }
    }
}
