using Unity.Entities;

namespace Icarus.Orbit {
    [RequireMatchingQueriesForUpdate]
    public partial class UpdateOrbitalPositionSystem : SystemBase {
        protected override void OnUpdate() {
            float dt = SystemAPI.Time.DeltaTime;

            Entities
                .ForEach(
                    (ref OrbitalParameters parms) => {
                        parms.Theta = parms.Theta + parms.DeltaTheta * dt;
                    })
                .ScheduleParallel();
        }
    }
}
