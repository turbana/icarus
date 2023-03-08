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
        public ComponentLookup<DatumString64> DatumStringLookup;

        [BurstCompile]
        protected override void OnCreate() {
            LocalTransformLookup = GetComponentLookup<LocalTransform>(false);
            DatumDoubleLookup = GetComponentLookup<DatumDouble>(true);
            DatumStringLookup = GetComponentLookup<DatumString64>(true);
        }
        
        [BurstCompile]
        protected override void OnUpdate() {
            LocalTransformLookup.Update(this);
            DatumDoubleLookup.Update(this);
            DatumStringLookup.Update(this);
            var LTL = LocalTransformLookup;
            var DDL = DatumDoubleLookup;
            var DSL = DatumStringLookup;

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
                    } else if (DSL.HasComponent(dref.Entity)) {
                        var datum = DSL[dref.Entity];
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
