using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Icarus.Orbit {
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(UpdateOrbitSystemGroup))]
    [UpdateBefore(typeof(UpdateOrbitalPositionSystem))]
    public partial class UpdateRotationSystem : SystemBase {
        protected override void OnUpdate() {
            OrbitalOptions opts = SystemAPI.GetSingleton<OrbitalOptions>();
            float dt = SystemAPI.Time.DeltaTime * opts.TimeScale;

            Entities
                .ForEach(
                    (ref RotationalParameters rot) => {
                        rot.ElapsedTime = (rot.ElapsedTime + dt) % rot.Period;
                        // y = radians rotated
                        float y = 2f * math.PI * rot.ElapsedTime / rot.Period;
                        rot.AxialRotation = quaternion.RotateY(-y);
                        // pos.LocalToParent = UniformScaleTransform
                        //     .FromPositionRotation(pos.LocalToParent.Position, rot.AxialRotation);
                        // quaternion rotation = math.mul(rot.Tilt, quaternion.RotateY(-y));
                        // var ltp = pos.LocalToParent;
                        // ltp.Rotation = math.mul(rot.Tilt, quaternion.RotateY(-y));
                        // pos.LocalToParent = ltp;
                        // pos.LocalToParent = UniformScaleTransform
                        //     .FromPositionRotation(pos.LocalToParent.Position, rotation);
                    })
                .ScheduleParallel();
        }
    }
}
