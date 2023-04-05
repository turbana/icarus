using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Transforms;

using Icarus.Mathematics;

namespace Icarus.Orbit {
    public struct OrbitalParameters : IComponentData {
        public double Period;
        public double Eccentricity;
        public double SemiMajorAxis;

        public double Inclination;
        public double AscendingNode;
        public dquaternion OrbitRotation;
        public FixedString64Bytes BodyName;
    }

    public struct OrbitalPosition : IComponentData {
        public double ElapsedTime;
        public double Theta;
        public double Altitude;
        public double3 LocalToWorld;
        public double3 LocalToParent;
    }

    // special orbits
    public struct PlayerOrbitTag : IComponentData {}
    public struct PlayerParentOrbitTag : IComponentData {}
    public struct PlayerSiblingOrbitTag : IComponentData {}

    // orbital body types
    public struct SunTag : IComponentData {}
    public struct PlanetTag : IComponentData {}
    public struct MoonTag : IComponentData {}
    public struct DwarfPlanetTag : IComponentData {}
    public struct AsteroidTag : IComponentData {}
    public struct ShipTag : IComponentData {}

    public enum OrbitTypeEnum {Planet, Moon, Ship};

    [AddComponentMenu("Icarus/Orbits/Orbital Parameters")]
    public class OrbitalParametersAuthoring : MonoBehaviour {
        public bool IsPlayer = false;
        public OrbitTypeEnum OrbitType;
        public double Period;
        public double Eccentricity;
        public double SemiMajorAxis;

        public double Inclination;
        public double AscendingNode;
        
        public double ElapsedTime;

        public static IComponentData OrbitTypeTag(OrbitTypeEnum t) => t switch {
            OrbitTypeEnum.Planet => new PlanetTag(),
            OrbitTypeEnum.Moon => new MoonTag(),
            OrbitTypeEnum.Ship => new ShipTag(),
            _ => throw new System.Exception("invalid OrbitTypeEnum")
        };

        public class Baker : Unity.Entities.Baker<OrbitalParametersAuthoring> {
            public override void Bake(OrbitalParametersAuthoring parms) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                if (parms.IsPlayer) {
                    AddComponent(entity, new PlayerOrbitTag());
                }
                switch(parms.OrbitType) {
                    case OrbitTypeEnum.Planet:
                        AddComponent(entity, new PlanetTag());
                        break;
                    case OrbitTypeEnum.Moon:
                        AddComponent(entity, new MoonTag());
                        break;
                    case OrbitTypeEnum.Ship:
                        AddComponent(entity, new ShipTag());
                        break;
                }
                AddComponent(entity, new OrbitalParameters {
                        Period = parms.Period,
                        Eccentricity = parms.Eccentricity,
                        SemiMajorAxis = parms.SemiMajorAxis,
                        Inclination = parms.Inclination,
                        AscendingNode = parms.AscendingNode,
                        OrbitRotation = dquaternion.EulerYXZ(
                            math.radians(parms.Inclination),
                            math.radians(parms.AscendingNode),
                            0f),
                        BodyName = (parms.IsPlayer) ? "HSS-423R" : "",
                    });
                AddComponent(entity, new OrbitalPosition {
                        ElapsedTime = parms.ElapsedTime,
                        LocalToParent = double3.zero,
                        LocalToWorld = double3.zero
                    });
            }
        }
    }
}
