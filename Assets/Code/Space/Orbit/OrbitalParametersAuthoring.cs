using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;

namespace Icarus.Orbit {
    public struct OrbitalParameters : IComponentData {
        public float Period;
        public float Eccentricity;
        public float SemiMajorAxis;

        public float Inclination;
        public float AscendingNode;
        
        public float TimeSincePerhelion;
        public float Theta;
        public float ParentDistance;
    }

    [AddComponentMenu("Icarus/Orbits/Orbital Parameters")]
    public class OrbitalParametersAuthoring : MonoBehaviour {
        public float Period;
        public float Eccentricity;
        public float SemiMajorAxis;

        public float Inclination;
        public float AscendingNode;
        
        public float TimeSincePerhelion;
        public float Theta;

        public class Baker : Unity.Entities.Baker<OrbitalParametersAuthoring> {
            public override void Bake(OrbitalParametersAuthoring parms) {
                AddComponent(new OrbitalParameters {
                        Period = parms.Period,
                        Eccentricity = parms.Eccentricity,
                        SemiMajorAxis = parms.SemiMajorAxis,
                        Inclination = parms.Inclination,
                        AscendingNode = parms.AscendingNode,
                        TimeSincePerhelion = parms.TimeSincePerhelion,
                        Theta = parms.Theta,
                        ParentDistance = 0f
                    });
            }
        }
    }
}
