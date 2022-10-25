using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Icarus.Orbit {
    [RequireMatchingQueriesForUpdate]
    [UpdateAfter(typeof(UpdateOrbitalPositionSystem))]
    public partial class UpdateOrbitalGamePositionSystem : SystemBase {
        protected override void OnUpdate() {
            Entities
                .ForEach(
                    (ref TransformAspect transform, in OrbitalParameters parms) => {
                        // quaternion rot = quaternion.RotateY(parms.Theta);
                        // float3 forward = new float3(0f, 0f, 1f);
                        // RigidTransform trans = new RigidTransform(rot, pos);
                        // Debug.Log($"{trans.pos}");
                        // transform.TranslateLocal(trans.pos);
                        // transform.LocalPosition = forward;
                        // RigidTransform trans = new RigidTransform(
                        //     quaternion.RotateY(0f), new float3(0f, 0f, 1f)
                        // ).RotateY(parms.Theta);
                        // RigidTransform trans = RigidTransform
                        //     .RotateY(parms.Theta)
                        //     .Translate(new float3(0f, 0f, 1f));
                        // transform.LocalPosition = trans.pos;
                        transform.LocalPosition = math.mul(quaternion.RotateY(parms.Theta), math.forward());
                    })
                .ScheduleParallel();
        }
    }
}
