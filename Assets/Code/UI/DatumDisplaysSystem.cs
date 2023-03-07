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

        [BurstCompile]
        protected override void OnCreate() {
            LocalTransformLookup = GetComponentLookup<LocalTransform>(false);
            DatumDoubleLookup = GetComponentLookup<DatumDouble>(true);
        }
        
        [BurstCompile]
        protected override void OnUpdate() {
            LocalTransformLookup.Update(this);
            DatumDoubleLookup.Update(this);
            var LTL = LocalTransformLookup;
            var DDL = DatumDoubleLookup;

            // update controls
            Entities
                .WithReadOnly(DDL)
                .ForEach((in ControlAspect control, in DatumRef dref) => {
                    var datum = DDL[dref.Entity];
                    LTL[control.Root] = control.GetLocalTransform(in datum);
                    })
                .Schedule();

            // update controls
            Entities
                .WithReadOnly(DDL)
                .ForEach((ManagedTextComponent text, in DatumRef dref, in TransformAspect pos) => {
                    var datum = DDL[dref.Entity];
                    // Debug.Log($"updating from datum: {datum.Value} ({text.Format})");
                    if (text.GO is null) text.CreateGameObject();
                    var tmp = text.TextMeshPro;
                    var rt = text.RectTransform;
                    // update text
                    tmp.text = String.Format(text.Format, datum.Value);
                    // update position / rotation / scale
                    rt.position = pos.WorldPosition;
                    rt.rotation = (Quaternion)pos.WorldRotation * Quaternion.Euler(0f, -90f, 0f);
                    rt.localScale = new Vector3(pos.WorldScale, pos.WorldScale, pos.WorldScale);
                    })
                .WithoutBurst()
                .Run();
        }
    }
}
