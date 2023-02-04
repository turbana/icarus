using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

using Icarus.Mathematics;

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
                        rot.ElapsedTime = (rot.ElapsedTime + (double)dt) % rot.Period;
                        // y = radians rotated
                        double y = math.radians(rot.ElapsedTime / rot.Period);
                        rot.AxialRotation = dquaternion.RotateY(-y);
                    })
                .ScheduleParallel();
        }
    }
}
