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
        private enum InteractionType {
            None, LeftClick, LeftClickDown, ScrollUp, ScrollDown, GiveControl
        };

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
            var scroll = Input.mouseScrollDelta[1];
            var interaction = new Interaction() {
                LeftClick = Input.GetMouseButtonDown(0),
                LeftClickDown = Input.GetMouseButton(0),
                ScrollUp = (scroll > 0f),
                ScrollDown = (scroll < 0f),
                GiveControl = Input.GetKeyDown("e"),
            };
            
            
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
                if (entity != Entity.Null) {
                    // Debug.Log("hit");
                    // Debug.Log($"hit entity={EntityManager.GetName(entity)}");
                    // do we have any input?
                    if (interaction.AnyInteraction) {
                        _InteractionLookup[entity] = interaction;
                        // Debug.Log($"interaction set");
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
