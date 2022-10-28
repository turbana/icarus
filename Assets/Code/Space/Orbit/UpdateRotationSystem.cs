using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Icarus.Orbit {
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(UpdateOrbitSystemGroup))]
    public partial class UpdateRotationSystem : SystemBase {
        protected override void OnUpdate() {
            OrbitalOptions opts = SystemAPI.GetSingleton<OrbitalOptions>();
            float dt = SystemAPI.Time.DeltaTime * opts.TimeScale;

            Entities
                .ForEach(
                    (ref TransformAspect transform, ref RotationalParameters parms) => {
                        parms.ElapsedTime = (parms.ElapsedTime + dt) % parms.SiderealRotationPeriod;
                        // y = radians rotated
                        float y = 2f * math.PI * parms.ElapsedTime / parms.SiderealRotationPeriod;
                        transform.LocalRotation = math.mul(parms.Tilt, quaternion.RotateY(-y));
                    })
                .ScheduleParallel();
        }
    }
}
