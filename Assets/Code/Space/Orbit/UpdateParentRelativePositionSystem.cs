using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Icarus.Orbit {
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(UpdateOrbitSystemGroup))]
    [UpdateAfter(typeof(UpdateOrbitalPositionSystem))]
    public partial class UpdateParentRelativePositionSystem : SystemBase {
        protected override void OnUpdate() {
            Entities
                .ForEach(
                    (ref TransformAspect transform, in OrbitalParameters parms) => {
                        quaternion orbit =
                            math.mul(quaternion.RotateY(math.radians(parms.AscendingNode)),
                                     quaternion.RotateX(math.radians(parms.Inclination)));
                        quaternion rot =
                            math.mul(orbit, quaternion.RotateY(-parms.Theta));
                        float3 pos = math.forward() * parms.ParentDistance;
                        transform.LocalPosition = math.mul(rot, pos);
                    })
                .ScheduleParallel();
        }
    }
}
