using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Icarus.Orbit {
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(UpdateOrbitSystemGroup))]
    [UpdateBefore(typeof(UpdateOrbitalPositionSystem))]
    public partial class UpdateParentRelativePositionSystem : SystemBase {
        protected override void OnUpdate() {
            Entities
                .ForEach((ref OrbitalParent parent) => {
                    OrbitalPosition parentPos = GetComponent<OrbitalPosition>(parent.Value);
                    parent.ParentToWorld = parentPos.LocalToWorld;
                })
                .ScheduleParallel();
        }
    }
}
