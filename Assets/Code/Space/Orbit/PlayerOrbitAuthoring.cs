using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

using Icarus.Mathematics;
using Icarus.Loading;

namespace Icarus.Orbit {
    [AddComponentMenu("Icarus/Loading/Player Orbit Component")]
    public class PlayerOrbitAuthoring : MonoBehaviour {
        public string ParentBody;
        [Tooltip("If true: the SemiMajorAxis will be multiplied by the ParentBody's radius")]
        public bool UseParentRadius = false;
        public float SemiMajorAxis = 0f;
        public float Inclination = 0f;
        public float Eccentricity = 0f;
        public float AscendingNode = 0f;
        public float ArgumentOfPeriapsis = 0f;
        public double ElapsedTime = 0.0;
        public double Mass = 419725; // ISS mass (kg)
        
        public class PlayerOrbitAuthoringBaker : Baker<PlayerOrbitAuthoring> {
            public override void Bake(PlayerOrbitAuthoring auth) {
                var db = Object.FindObjectOfType(typeof(OrbitalDatabaseAuthoring)) as OrbitalDatabaseAuthoring;
                OrbitalDatabaseData parent;
                try {
                    parent = db.LookupBody(auth.ParentBody);
                } catch (System.ArgumentException) {
                    return;
                }
                float sma = auth.SemiMajorAxis;
                if (auth.UseParentRadius) {
                    sma *= parent.Radius;
                }
                // find orbital period from semi-major axis and (total) mass
                // see: https://en.wikipedia.org/wiki/Kepler%27s_laws_of_planetary_motion#Third_law
                // T^2 = r^3 * (4PI^2 / GM)
                double period = System.Math.Sqrt(System.Math.Pow((double)sma * 1000, 3) * ((4 * dmath.PI * dmath.PI) / (dmath.G * (parent.Mass + auth.Mass))));
                // Debug.Log($"orbiting {parent.Name} at {sma}km with period {period} ({parent.Mass} / {auth.Mass})");
                AddComponent<PlayerOrbitTag>();
                AddComponent<ShipTag>();
                AddComponent<OrbitalParameters>(new OrbitalParameters {
                        Period = period,
                        Eccentricity = auth.Eccentricity,
                        SemiMajorAxis = sma,
                        Inclination = auth.Inclination,
                        AscendingNode = auth.AscendingNode,
                        // TODO argument of periapsis
                        OrbitRotation = dquaternion.EulerYXZ(math.radians(auth.Inclination),
                                                             math.radians(auth.AscendingNode),
                                                             0)
                    });
                AddComponent<OrbitalPosition>(new OrbitalPosition {
                        ElapsedTime = auth.ElapsedTime,
                        LocalToParent = double3.zero,
                        LocalToWorld = double3.zero
                    });
                AddComponent<AddOrbitalParent>(new AddOrbitalParent {
                        Value = parent.Name
                    });
            }
        }
    }
}
