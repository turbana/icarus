using UnityEngine;

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace Icarus.UI {
    [BurstCompile]
    [UpdateInGroup(typeof(UserInputSystemGroup))]
    public partial class QueryUserInputSystem : SystemBase {
        private ComponentLookup<DatumByte> DatumLookup;
        private Camera MainCamera;

        [BurstCompile]
        protected override void OnCreate() {
            DatumLookup = GetComponentLookup<DatumByte>(false);
            MainCamera = Camera.main;
        }
        
        protected override void OnUpdate() {
            DatumLookup.Update(this);
            
            // read user input
            var inputs = Interaction.FromUserInput();
            
            var pworld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            var crosshair = SystemAPI.ManagedAPI.GetSingleton<Crosshair>();
            var next_crosshair = new NativeList<CrosshairType>(1, Allocator.TempJob);
            next_crosshair.Add(CrosshairType.Normal);
            float3 rstart = MainCamera.transform.position;
            float3 rend = rstart + (float3)(MainCamera.transform.forward * Constants.INTERACT_DISTANCE);
            // Debug.Log($"ray casting from {rstart} to {rend} mask={INTERACTION_LAYER_MASK}");
            var DL = DatumLookup;
            
            Job.WithCode(() => {
                Raycast(out Entity entity, pworld, rstart, rend);
                // did we hit a collider?
                if (entity != Entity.Null) {
                    // Debug.Log("hit");
                    var control = SystemAPI.GetAspectRO<ControlAspect>(entity);
                    var datum = DL[control.Datum];
                    // find next value
                    var next = control.NextValue(in datum, in inputs);
                    // update datum
                    if (next != datum.Value) {
                        datum.PreviousValue = datum.Value;
                        datum.Value = next;
                        // Debug.Log($"setting datum to {datum.Value} (old {datum.PreviousValue})");
                        DL[control.Datum] = datum;
                    }
                    // set crosshair
                    next_crosshair[0] = control.Crosshair(in datum);
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
