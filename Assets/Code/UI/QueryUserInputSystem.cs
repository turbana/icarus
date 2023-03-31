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
        private Camera MainCamera;
        ComponentLookup<Crosshair> CrosshairLookup;

        [BurstCompile]
        protected override void OnCreate() {
            MainCamera = Camera.main;
            CrosshairLookup = GetComponentLookup<Crosshair>(false);
        }
        
        [BurstCompile]
        protected override void OnUpdate() {
            CrosshairLookup.Update(this);
            var CL = CrosshairLookup;

            // crosshair entitiy
            var centity = SystemAPI.GetSingletonEntity<Crosshair>();
            
            // read user input
            var inputs = Interaction.FromUserInput();

            // setup physics ray caster
            var pworld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            var comp = SystemAPI.GetSingletonRW<Crosshair>();
            float3 rstart = MainCamera.transform.position;
            float3 rend = rstart + (float3)(MainCamera.transform.forward * Constants.INTERACT_DISTANCE);
            
            Entities
                .ForEach((ref DatumCollection datums) => {
                    Raycast(out Entity entity, pworld, rstart, rend);
                    var crosshair = CrosshairType.Normal;
                    // did we hit a collider?
                    if (entity != Entity.Null) {
                        var aspect = SystemAPI.HasComponent<ControlSettings>(entity)
                            && SystemAPI.HasComponent<DatumRef>(entity);
                        // Debug.Log($"hit {aspect}");
                        if (aspect) {
                            var control = SystemAPI.GetAspectRO<ControlAspect>(entity);
                            if (inputs.AnyInteraction) {
                                var value = datums.GetDouble(control.Datum, 0);
                                // find next value
                                var next = control.NextValue(value, in inputs);
                                // update datum
                                if (next != value) {
                                    datums.SetDouble(control.Datum, next);
                                }
                            }
                            crosshair = control.Crosshair();
                        }
                    }
                    CL[centity] = new Crosshair { Value = crosshair };
                })
                .Schedule();

            // NOTE: we need to complete before the character controller
            // runs. We can either add ourselves as a dependency (or vice
            // versa), but I don't know a way to do that. So to ensure we don't
            // clobber each other in the physics world ensure we're complete
            // before moving on.
            this.Dependency.Complete();
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
