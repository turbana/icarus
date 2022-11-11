using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Transforms;

namespace Icarus.Orbit {
    public struct OrbitalParameters : IComponentData {
        public float Period;
        public float Eccentricity;
        public float SemiMajorAxis;

        public float Inclination;
        public float AscendingNode;
        public quaternion OrbitRotation;
    }

    public struct OrbitalPosition : IComponentData {
        public float ElapsedTime;
        public float Theta;
        public float Altitude;
        public UniformScaleTransform LocalToWorld;
        public UniformScaleTransform LocalToParent;
    }

    public struct PlayerOrbitTag : IComponentData {}
    public struct PlanetTag : IComponentData {}
    public struct MoonTag : IComponentData {}
    public struct ShipTag : IComponentData {}

    public enum OrbitTypeEnum {Planet, Moon, Ship};

    [AddComponentMenu("Icarus/Orbits/Orbital Parameters")]
    public class OrbitalParametersAuthoring : MonoBehaviour {
        public bool IsPlayer = false;
        public OrbitTypeEnum OrbitType;
        public float Period;
        public float Eccentricity;
        public float SemiMajorAxis;

        public float Inclination;
        public float AscendingNode;
        
        public float TimeSincePerhelion;
        public float Theta;

        public static IComponentData OrbitTypeTag(OrbitTypeEnum t) => t switch {
            OrbitTypeEnum.Planet => new PlanetTag(),
            OrbitTypeEnum.Moon => new MoonTag(),
            OrbitTypeEnum.Ship => new ShipTag(),
            _ => throw new System.Exception("invalid OrbitTypeEnum")
        };

        public class Baker : Unity.Entities.Baker<OrbitalParametersAuthoring> {
            public override void Bake(OrbitalParametersAuthoring parms) {
                if (parms.IsPlayer) {
                    AddComponent(new PlayerOrbitTag());
                }
                switch(parms.OrbitType) {
                    case OrbitTypeEnum.Planet:
                        AddComponent(new PlanetTag());
                        break;
                    case OrbitTypeEnum.Moon:
                        AddComponent(new MoonTag());
                        break;
                    case OrbitTypeEnum.Ship:
                        AddComponent(new ShipTag());
                        break;
                }
                AddComponent(new OrbitalParameters {
                        Period = parms.Period,
                        Eccentricity = parms.Eccentricity,
                        SemiMajorAxis = parms.SemiMajorAxis,
                        Inclination = parms.Inclination,
                        AscendingNode = parms.AscendingNode,
                        OrbitRotation = quaternion.EulerYXZ(math.radians(parms.Inclination),
                                                            math.radians(parms.AscendingNode),
                                                            0f)
                    });
                AddComponent(new OrbitalPosition());
            }
        }
    }
}
