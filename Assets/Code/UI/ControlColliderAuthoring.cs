using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Icarus.UI {
    public class ControlColliderAuthoring : MonoBehaviour {
        [Tooltip("The type of this collider")]
        public InteractionControlType Type;

        public class ControlColliderAuthoringBaker : Baker<ControlColliderAuthoring> {
            public override void Bake(ControlColliderAuthoring auth) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                var control = auth.gameObject.GetComponentInParent<BaseControlAuthoring>();
                if (control == null) {
                    Debug.LogError($"no BaseControlAuthoring found on any parent of game object: {auth.gameObject}", auth.gameObject);
                    return;
                }
                var go = control.gameObject;
                DependsOn(control);
                DependsOn(go);
                // we search the GameObject hierarchy for the datum id, so mark
                // them as dependencies.
                var pcomp = GetComponentInParent<ControlDatumPrefixAuthoring>();
                if (pcomp != null) DependsOn(pcomp);
                var scomp = GetComponentInChildren<ControlLabelAuthoring>();
                if (scomp != null) DependsOn(scomp);
                var pos = go.transform.localPosition;
                var rot = go.transform.localRotation;
                var scale = go.transform.localScale;
                if (scale.x != scale.y || scale.y != scale.z) {
                    Debug.LogError($"only uniform scaling is supported for controls: {go}", go);
                    return;
                }
                var settings = new ControlSettings {
                    Stops = 2,
                    Rotation = 0f,
                    Movement = float3.zero,
                    Type = auth.Type,
                    Root = GetEntity(go, TransformUsageFlags.Dynamic),
                    InitialTransform = LocalTransform.FromPositionRotationScale(pos, rot, scale.x),
                };
                if (control is MultiWayControlAuthoring) {
                    var mcontrol = control as MultiWayControlAuthoring;
                    settings.Rotation = (mcontrol.RotateAngle * Mathf.Deg2Rad) / (mcontrol.Stops - 1);
                    settings.Stops = mcontrol.Stops;
                } else if (control is SimpleButtonAuthoring) {
                    var bcontrol = control as SimpleButtonAuthoring;
                    settings.Movement = bcontrol.Movement;
                }
                AddComponent<ControlSettings>(entity, settings);
                AddComponent<DatumRef>(entity, new DatumRef {
                        Name = control.DatumID,
                        Type = DatumType.Double,
                    });
                if (control.DatumID == "" || control.DatumID == ".") {
                    Debug.LogWarning($"found an empty DatumID on game object: {control.gameObject}", control.gameObject);
                }
            }
        }
    }
}
