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
        public ComponentLookup<DatumDouble> DatumDoubleLookup;
        public ComponentLookup<DatumString64> DatumString64Lookup;
        public ComponentLookup<DatumString512> DatumString512Lookup;

        [BurstCompile]
        protected override void OnCreate() {
            LocalTransformLookup = GetComponentLookup<LocalTransform>(false);
            DatumDoubleLookup = GetComponentLookup<DatumDouble>(true);
            DatumString64Lookup = GetComponentLookup<DatumString64>(true);
            DatumString512Lookup = GetComponentLookup<DatumString512>(true);
        }
        
        [BurstCompile]
        protected override void OnUpdate() {
            LocalTransformLookup.Update(this);
            DatumDoubleLookup.Update(this);
            DatumString64Lookup.Update(this);
            DatumString512Lookup.Update(this);
            
            var LTL = LocalTransformLookup;
            var DDL = DatumDoubleLookup;
            var DS64L = DatumString64Lookup;
            var DS512L = DatumString512Lookup;

            // update controls
            Entities
                .WithReadOnly(DDL)
                .ForEach((in ControlAspect control, in DatumRef dref) => {
                    var datum = DDL[dref.Entity];
                    LTL[control.Root] = control.GetLocalTransform(in datum);
                })
                .Schedule();

            // update dynamic text
            Entities
                .WithReadOnly(DDL)
                .ForEach((ManagedTextComponent text, in DatumRef dref, in TransformAspect pos) => {
                    text.UpdatePosition(in pos);
                    if (DDL.HasComponent(dref.Entity)) {
                        var datum = DDL[dref.Entity];
                        text.UpdateText(datum.Value);
                    } else if (DS64L.HasComponent(dref.Entity)) {
                        var datum = DS64L[dref.Entity];
                        text.UpdateText(datum.Value);
                    } else if (DS512L.HasComponent(dref.Entity)) {
                        var datum = DS512L[dref.Entity];
                        text.UpdateText(datum.Value);
                    }
                })
                .WithoutBurst()
                .Run();

            // update static text
            Entities
                .WithNone<DatumRef>()
                // XXX why can't we use a change filter here?
                // .WithChangeFilter<ManagedTextComponent>()
                .ForEach((ManagedTextComponent text, in TransformAspect pos) => {
                    text.UpdateText(System.Double.MaxValue);
                    text.UpdatePosition(in pos);
                })
                .WithoutBurst()
                .Run();
        }
    }
}
