using UnityEngine;

using Unity.Burst;
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
            None, LeftClick, ScrollUp, ScrollDown, Interact
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
            var interact = InteractionType.None;
            var scroll = Input.mouseScrollDelta[1];

            if (Input.GetMouseButtonDown(0)) {
                interact = InteractionType.LeftClick;
            } else if (Input.GetKeyDown("e")) {
                interact = InteractionType.Interact;
            } else if (scroll > 0f) {
                interact = InteractionType.ScrollUp;
            } else if (scroll < 0f) {
                interact = InteractionType.ScrollDown;
            }
            
            if (interact != InteractionType.None) {
                float3 rstart = MainCamera.transform.position;
                float3 rend = rstart + (float3)(MainCamera.transform.forward * Constants.INTERACT_DISTANCE);
                // Debug.Log($"ray casting from {rstart} to {rend} mask={INTERACTION_LAYER_MASK}");
                var pworld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
                var _InteractionLookup = InteractionLookup;
                Job.WithCode(() => {
                    Entity entity;
                    // var entity = ref Raycast(ref pworld, rstart, rend);
                    Raycast(out entity, pworld, rstart, rend);
                    if (entity != Entity.Null) {
                        // Debug.Log($"hit");
                        _InteractionLookup[entity] = new Interaction() {
                            Toggle = (interact == InteractionType.LeftClick),
                            ScrollUp = (interact == InteractionType.ScrollUp),
                            ScrollDown = (interact == InteractionType.ScrollDown),
                            GiveControl = (interact == InteractionType.Interact),
                        };
                    } else {
                        // Debug.Log("no hit");
                    }
                }).Schedule();
            }
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
