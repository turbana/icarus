using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;

namespace Icarus.Orbit {
    public struct RotationalParameters : IComponentData {
        public float AxialTilt;
        public float NorthPoleRA;
        public float SiderealRotationPeriod;
        public float ElapsedTime;
        public quaternion Tilt;
    }

    [AddComponentMenu("Icarus/Orbits/Rotational Parameters")]
    public class RotationalParametersAuthoring : MonoBehaviour {
        public float AxialTilt;
        public float NorthPoleRA;
        public float SiderealRotationPeriod;
        public float ElapsedTime;

        public class Baker : Unity.Entities.Baker<RotationalParametersAuthoring> {
            public override void Bake(RotationalParametersAuthoring parms) {
                quaternion tilt = math
                    .mul(quaternion.RotateX(math.radians(parms.AxialTilt)),
                         quaternion.RotateY(-parms.NorthPoleRA));
                AddComponent(new RotationalParameters {
                        AxialTilt = parms.AxialTilt,
                        NorthPoleRA = parms.NorthPoleRA,
                        SiderealRotationPeriod = parms.SiderealRotationPeriod,
                        Tilt = tilt,
                        ElapsedTime = parms.ElapsedTime
                    });
            }
        }
    }
}
