using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Icarus.Orbit {
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(UpdateOrbitSystemGroup))]
    public partial class UpdateOrbitalPositionSystem : SystemBase {
        protected override void OnUpdate() {
            OrbitalOptions opts = SystemAPI.GetSingleton<OrbitalOptions>();
            float dt = SystemAPI.Time.DeltaTime * opts.TimeScale;

            Entities
                .ForEach(
                    (Entity entity, ref OrbitalPosition pos,
                     in OrbitalParent parent, in OrbitalParameters parms) => {
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
                        // update position within orbit
                        quaternion rot = math.mul(parms.OrbitRotation, quaternion.RotateY(-pos.Theta));
                        // rot = math.mul(rot, parent.ParentToWorld.Rotation);
                        quaternion ptilt = GetComponent<RotationalParameters>(parent.Value).AxialTilt;
                        rot = math.mul(ptilt, rot);
                        // pos.LocalToParent.Position = math.mul(rot, math.forward() * pos.Altitude);
                        float3 loc = math.mul(rot, math.forward() * pos.Altitude);
                        var ltp = pos.LocalToParent;
                        ltp.Position = loc;
                        // ltp.Position = parent.ParentToWorld.TransformPoint(loc);
                        pos.LocalToParent = ltp;
                        // pos.LocalToWorld = pos.LocalToParent.TransformTransform(parent.ParentToWorld);
                        pos.LocalToWorld = parent.ParentToWorld.TransformTransform(pos.LocalToParent);
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
