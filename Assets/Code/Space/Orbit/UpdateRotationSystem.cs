using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Icarus.Orbit {
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(UpdateOrbitSystemGroup))]
    [UpdateBefore(typeof(UpdateParentRelativePositionSystem))]
    public partial class UpdateRotationSystem : SystemBase {
        protected override void OnUpdate() {
            // XXX
            // return;
            OrbitalOptions opts = SystemAPI.GetSingleton<OrbitalOptions>();
            float dt = SystemAPI.Time.DeltaTime * opts.TimeScale;

            Entities
                .ForEach(
                    (ref OrbitalPosition pos, ref RotationalParameters rot) => {
                        rot.ElapsedTime = (rot.ElapsedTime + dt) % rot.SiderealRotationPeriod;
                        // y = radians rotated
                        float y = 2f * math.PI * rot.ElapsedTime / rot.SiderealRotationPeriod;
                        var ltp = pos.LocalToParent;
                        ltp.Rotation = math.mul(rot.Tilt, quaternion.RotateY(-y));
                        pos.LocalToParent = ltp;
                    })
                .ScheduleParallel();
        }
    }
}
