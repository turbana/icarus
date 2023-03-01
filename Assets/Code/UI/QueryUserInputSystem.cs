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
        private ComponentLookup<LocalToWorld> LTWLookup;
        private ComponentLookup<Interaction> InteractionLookup;
        private Camera MainCamera;

        [BurstCompile]
        protected override void OnCreate() {
            LTWLookup = GetComponentLookup<LocalToWorld>(true);
            InteractionLookup = GetComponentLookup<Interaction>(false);
            MainCamera = Camera.main;
        }
        
        protected override void OnUpdate() {
            LTWLookup.Update(this);
            InteractionLookup.Update(this);
            
            // read user input
            var inputs = Interaction.FromUserInput();
            
            float3 rstart = MainCamera.transform.position;
            float3 rend = rstart + (float3)(MainCamera.transform.forward * Constants.INTERACT_DISTANCE);
            // Debug.Log($"ray casting from {rstart} to {rend} mask={INTERACTION_LAYER_MASK}");
            var pworld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            var _InteractionLookup = InteractionLookup;
            var output = new NativeList<CrosshairType>(1, Allocator.TempJob);
            output.Add(CrosshairType.Normal);
            
            Job.WithCode(() => {
                Entity entity;
                Raycast(out entity, pworld, rstart, rend);
                // did we hit a collider?
                if (entity != Entity.Null) {
                    // Debug.Log("hit");
                    // Debug.Log($"hit entity={EntityManager.GetName(entity)}");
                    // do we have any input?
                    if (inputs.AnyInteraction) {
                        // does this collider consume the input?
                        var collider = _InteractionLookup[entity];
                        if (collider.CanConsume(inputs)) {
                            // assign the user input
                            collider.Value = inputs.Value;
                            _InteractionLookup[entity] = collider;
                            // Debug.Log($"interaction set");
                        }
                    }
                    // update crosshair
                    output[0] = CrosshairType.Toggle;
                } else {
                    // Debug.Log("no hit");
                }
            }).Schedule();
                
            this.Dependency.Complete();
            
            // update crosshair
            var xhair = SystemAPI.ManagedAPI.GetSingleton<Crosshair>();
            xhair.Value = output[0];
            // UnityEngine.Debug.Log($"crosshair = {output[0]}");
            output.Dispose();
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
