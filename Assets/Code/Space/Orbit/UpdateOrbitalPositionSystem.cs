using Unity.Entities;
using Unity.Mathematics;

namespace Icarus.Orbit {
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(UpdateOrbitSystemGroup))]
    public partial class UpdateOrbitalPositionSystem : SystemBase {
        protected override void OnUpdate() {
            OrbitalOptions opts = SystemAPI.GetSingleton<OrbitalOptions>();
            float dt = SystemAPI.Time.DeltaTime * opts.TimeScale;

            Entities
                .ForEach(
                    (ref OrbitalPosition pos, in OrbitalParameters parms) => {
                        // update elapsed time
                        pos.ElapsedTime = (pos.ElapsedTime + dt) % parms.Period;
                        // mean motion
                        float n = (2f * math.PI) / parms.Period;
                        // mean anomaly
                        float M = n * pos.ElapsedTime;
                        // eccentric anomaly
                        float e = parms.Eccentricity;
                        float E = EccentricAnomaly(M, e);
                        // true anomaly
                        // https://en.wikipedia.org/wiki/True_anomaly#From_the_eccentric_anomaly
                        float beta = e / (1f + math.sqrt(1 - math.pow(e, 2f)));
                        pos.Theta = E + 2f * math.atan((beta * math.sin(E)) / (1f - beta * math.cos(E)));
                        // parent distance
                        pos.Altitude = parms.SemiMajorAxis * (1f - e * math.cos(E));
                    })
                .ScheduleParallel();
        }
        
        // taken from: https://squarewidget.com/keplers-equation/
        // which itself was taken from: Meeus, Jean. Astronomical Algorithms. 2nd Ed. Willmann-Bell. 1998. (p. 199)
        private static float EccentricAnomaly(float M, float e) {
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
