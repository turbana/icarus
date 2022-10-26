using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Icarus.Orbit {
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(UpdateOrbitSystemGroup))]
    public partial class UpdateRotationSystem : SystemBase {
        protected override void OnUpdate() {
            float dt = SystemAPI.Time.DeltaTime * 1000000f;

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
