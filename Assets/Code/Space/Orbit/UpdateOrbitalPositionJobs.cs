using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Icarus.Orbit {
    // update rotational parameters
    public partial struct UpdateRotationJob : IJobEntity {
        public float dt;
        
        void Execute(ref TransformAspect transform, ref RotationalParameters parms) {
            parms.ElapsedTime = (parms.ElapsedTime + dt) % parms.SiderealRotationPeriod;
            // y = radians rotated
            float y = 2f * math.PI * parms.ElapsedTime / parms.SiderealRotationPeriod;
            transform.LocalRotation = math.mul(parms.Tilt, quaternion.RotateY(-y));
        }
    }
    

    // update orbital positions
    // math taken from: https://en.wikipedia.org/wiki/Kepler%27s_laws_of_planetary_motion#Position_as_a_function_of_time
    public partial struct UpdateOrbitalPositionJob : IJobEntity {
        public float dt;
        
        void Execute(ref OrbitalParameters parms) {
            // update elapsed time
            parms.TimeSincePerhelion = (parms.TimeSincePerhelion + dt) % parms.Period;
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
        
        // taken from: https://squarewidget.com/keplers-equation/
        // which itself was taken from: Meeus, Jean. Astronomical Algorithms. 2nd Ed. Willmann-Bell. 1998. (p. 199)
        private float EccentricAnomaly(float M, float e) {
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

    
    // update parent-relative positions
    // get player sun-relative position

    
    // set player-relative game object position
    public partial struct UpdateGamePositionJob : IJobEntity {
        void Execute(ref TransformAspect transform, in OrbitalParameters parms) {
            quaternion orbit =
                math.mul(quaternion.RotateY(math.radians(parms.AscendingNode)),
                         quaternion.RotateX(math.radians(parms.Inclination)));
            quaternion rot =
                math.mul(orbit, quaternion.RotateY(-parms.Theta));
            float3 pos = math.forward() * parms.ParentDistance;
            transform.LocalPosition = math.mul(rot, pos);
        }
    }
}
