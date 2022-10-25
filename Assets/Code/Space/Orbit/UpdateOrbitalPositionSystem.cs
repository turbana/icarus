using Unity.Entities;
using Unity.Mathematics;

namespace Icarus.Orbit {
    [RequireMatchingQueriesForUpdate]
    public partial class UpdateOrbitalPositionSystem : SystemBase {
        protected override void OnUpdate() {
            float dt = SystemAPI.Time.DeltaTime;

            Entities
                .ForEach(
                    (ref OrbitalParameters parms) => {
                        parms.TimeSincePerhelion = (parms.TimeSincePerhelion + dt * 100000f) % parms.Period;
                        UpdatePosition(ref parms);
                    })
                .ScheduleParallel();
        }

        internal static void UpdatePosition(ref OrbitalParameters parms) {
            // math taken from: https://en.wikipedia.org/wiki/Kepler%27s_laws_of_planetary_motion#Position_as_a_function_of_time
            // mean motion
            float n = (2f * math.PI) / parms.Period;
            // mean anomaly
            float M = n * parms.TimeSincePerhelion;
            // eccentric anomaly
            float e = parms.Eccentricity;
            float E = EccentricAnomaly(M, e);
            // true anomaly
            // https://en.wikipedia.org/wiki/True_anomaly#From_the_eccentric_anomaly
            float beta = e / (1f + math.sqrt(1 - math.pow(e, 2f)));
            float v = E + 2f * math.atan((beta * math.sin(E)) / (1f - beta * math.cos(E)));
            // parent distance
            float r = parms.SemiMajorAxis * (1f - e * math.cos(E));
            // update parms
            parms.Theta = v;
            parms.ParentDistance = r;
        }

        protected static float EccentricAnomaly(float M, float e) {
            float E0 = M;
            float E1 = 0;
            for(int i = 0; i < 6; i++) {
                E1 = E0 + (math.mad(e, math.sin(E0), M) - E0) /
                          (1f - e * math.cos(E0));
                E0 = E1;
            }
            return E0;
        }
    }
}
