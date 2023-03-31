using System;

using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Icarus.UI {
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(IcarusPresentationSystemGroup))]
    public partial class DatumDisplaysSystem : SystemBase {
        public ComponentLookup<LocalTransform> LocalTransformLookup;

        [BurstCompile]
        protected override void OnCreate() {
            LocalTransformLookup = GetComponentLookup<LocalTransform>(false);
        }
        
        [BurstCompile]
        protected override void OnUpdate() {
            LocalTransformLookup.Update(this);
            var LTL = LocalTransformLookup;
            this.Dependency.Complete();
            var datums = SystemAPI.GetSingleton<DatumCollection>();

            // update controls
            Entities
                .ForEach((in ControlAspect control, in DatumRef dref) => {
                    double value = 0;
                    if (datums.HasDatum(dref.Name)) {
                        value = datums.GetDouble(dref.Name);
                    }
                    LTL[control.Root] = control.GetLocalTransform((float)value);
                })
                .Schedule();
            
            // update dynamic text
            Entities
                .ForEach((ManagedTextComponent text, in DatumRef dref, in LocalToWorld pos) => {
                    // skip datums that have yet to be set
                    if (!datums.HasDatum(dref.Name)) return;
                    // UnityEngine.Debug.Log($"looking up {dref.Name}");
                    text.UpdatePosition(in pos);
                    switch (dref.Type) {
                        case DatumType.Double:
                            text.UpdateText(datums.GetDouble(dref.Name));
                            break;
                        case DatumType.String64:
                            text.UpdateText(datums.GetString64(dref.Name));
                            break;
                        case DatumType.String512:
                            text.UpdateText(datums.GetString512(dref.Name));
                            break;
                    }
                })
                .WithoutBurst()
                .Run();

            // update static text
            Entities
                .WithNone<DatumRef>()
                // XXX why can't we use a change filter here?
                // .WithChangeFilter<ManagedTextComponent>()
                .ForEach((ManagedTextComponent text, in LocalToWorld pos) => {
                    text.UpdateText(System.Double.MaxValue);
                    text.UpdatePosition(in pos);
                })
                .WithoutBurst()
                .Run();
        }
    }
}
