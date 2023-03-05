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
                var control = auth.gameObject.GetComponentInParent<BaseControlAuthoring>();
                if (control == null) {
                    Debug.LogError($"no BaseControlAuthoring found on any parent of game object: {auth.gameObject}", auth.gameObject);
                    return;
                }
                var go = control.gameObject;
                DependsOn(control);
                DependsOn(go);
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
                    Root = GetEntity(go),
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
                AddComponent<ControlSettings>(settings);
                AddComponent<UninitializedDatumRef>(new UninitializedDatumRef {
                        ID = control.DatumID,
                        Type = DatumType.Byte,
                    });
                if (control.DatumID == "") {
                    Debug.LogWarning($"found an empty DatumID on game object: {control.gameObject}", control.gameObject);
                }
            }
        }
    }
}
