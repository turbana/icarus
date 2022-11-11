using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Icarus.Orbit {
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(UpdateOrbitSystemGroup))]
    [UpdateAfter(typeof(UpdateOrbitalPositionSystem))]
    public partial class UpdateParentRelativePositionSystem : SystemBase {
        protected override void OnUpdate() {
            var bodies = Entities
                .ForEach(
                    (ref OrbitalPosition pos, in OrbitalParent parent, in OrbitalParameters parms) => {
                        // quaternion orbit =
                        //     math.mul(quaternion.RotateY(math.radians(parms.AscendingNode)),
                        //              quaternion.RotateX(math.radians(parms.Inclination)));
                        // quaternion orbit =
                        //     quaternion.EulerYXZ(math.radians(parms.Inclination),
                        //                         math.radians(parms.AscendingNode),
                        //                         0f);
                        // quaternion rot =
                        //     math.mul(orbit, quaternion.RotateY(-parms.Theta));
                        quaternion rot = quaternion
                            .EulerYXZ(math.radians(parms.Inclination),
                                      // -parms.Theta,
                                      -pos.Theta + math.radians(parms.AscendingNode),
                                      0f);
                        // quaternion orbit =
                        //     quaternion.EulerYXZ(math.radians(parms.AscendingNode),
                        //                         math.radians(parms.Inclination),
                        //                         0f);
                        // quaternion rot =
                        //     quaternion.RotateY(-parms.Theta);
                        // float3 pos = math.forward() * parms.ParentDistance;
                        // parms.ParentPosition = math.mul(rot, pos);
                        // parms.SolarPosition = parms.ParentPosition;
                        pos.LocalToParent.Position = math.mul(rot, math.forward() * pos.Altitude);
                        pos.LocalToWorld = pos.LocalToParent.TransformTransform(parent.ParentToWorld);
                    })
                .ScheduleParallel(this.Dependency);
            
            // save a copy of SolarPosition so that child nodes can access it easily
            // var fixup = Entities
            //     .ForEach(
            //         (ref FixupSolarPosition fixup, in OrbitalParameters parms) => {
            //             fixup.SolarPosition = parms.SolarPosition;
            //         })
            //     .ScheduleParallel(bodies);
            
            this.Dependency = bodies;
        }
    }
}
