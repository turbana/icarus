using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;

namespace Icarus.Orbit {
    public struct RotationalParameters : IComponentData {
        public float Tilt;
        public float NorthPoleRA;
        public float Period;
        public float ElapsedTime;
        public quaternion AxialTilt;
        public quaternion AxialRotation;
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
                    .mul(quaternion.RotateX(-math.radians(parms.AxialTilt)),
                         quaternion.RotateY(-math.radians(parms.NorthPoleRA)));
                AddComponent(new RotationalParameters {
                        Tilt = parms.AxialTilt,
                        NorthPoleRA = parms.NorthPoleRA,
                        Period = parms.SiderealRotationPeriod,
                        AxialTilt = tilt,
                        AxialRotation = quaternion.EulerXYZ(0f),
                        ElapsedTime = parms.ElapsedTime
                    });
            }
        }
    }
}
