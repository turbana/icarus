using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Icarus.Orbit {
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(UpdateOrbitSystemGroup))]
    [UpdateAfter(typeof(UpdateParentRelativePositionSystem))]
    public partial class UpdateSolarPositionSystem : SystemBase {
        protected override void OnUpdate() {
            var moons = Entities
                .WithAll<MoonTag>()
                .ForEach(
                    (ref OrbitalParameters parms, in OrbitalParent parent) => {
                        FixupSolarPosition fixup = GetComponent<FixupSolarPosition>(parent.Value);
                        parms.SolarPosition = fixup.SolarPosition + parms.SolarPosition;
                    })
                .ScheduleParallel(this.Dependency);
            
            var ships = Entities
                .WithAll<ShipTag>()
                .ForEach(
                    (ref OrbitalParameters parms, in OrbitalParent parent) => {
                        FixupSolarPosition fixup = GetComponent<FixupSolarPosition>(parent.Value);
                        parms.SolarPosition = fixup.SolarPosition + parms.SolarPosition;
                    })
                .ScheduleParallel(moons);

            this.Dependency = ships;
        }
    }
}
