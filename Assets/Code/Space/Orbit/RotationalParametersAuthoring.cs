using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;

using Icarus.Mathematics;

namespace Icarus.Orbit {
    public struct RotationalParameters : IComponentData {
        public double Tilt;
        public double NorthPoleRA;
        public double Period;
        public double ElapsedTime;
        public dquaternion AxialTilt;
        public dquaternion AxialRotation;
    }

    [AddComponentMenu("Icarus/Orbits/Rotational Parameters")]
    public class RotationalParametersAuthoring : MonoBehaviour {
        public double AxialTilt;
        public double NorthPoleRA;
        public double SiderealRotationPeriod;
        public double ElapsedTime;

        public class Baker : Unity.Entities.Baker<RotationalParametersAuthoring> {
            public override void Bake(RotationalParametersAuthoring parms) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                dquaternion tilt = dmath
                    .mul(dquaternion.RotateX(-math.radians(parms.AxialTilt)),
                         dquaternion.RotateY(-math.radians(parms.NorthPoleRA)));
                AddComponent(entity, new RotationalParameters {
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
