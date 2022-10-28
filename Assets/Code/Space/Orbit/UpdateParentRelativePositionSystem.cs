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
                    (ref OrbitalParameters parms) => {
                        quaternion orbit =
                            math.mul(quaternion.RotateY(math.radians(parms.AscendingNode)),
                                     quaternion.RotateX(math.radians(parms.Inclination)));
                        quaternion rot =
                            math.mul(orbit, quaternion.RotateY(-parms.Theta));
                        float3 pos = math.forward() * parms.ParentDistance;
                        parms.SolarPosition = math.mul(rot, pos);
                    })
                .ScheduleParallel(this.Dependency);
            
            // save a copy of SolarPosition so that child nodes can access it easily
            var fixup = Entities
                .ForEach(
                    (ref FixupSolarPosition fixup, in OrbitalParameters parms) => {
                        fixup.SolarPosition = parms.SolarPosition;
                    })
                .ScheduleParallel(bodies);
            
            this.Dependency = fixup;
        }
    }
}
