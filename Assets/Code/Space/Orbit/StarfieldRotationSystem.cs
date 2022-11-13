using Unity.Entities;
using Unity.Transforms;

using Icarus.Misc;

namespace Icarus.Orbit {
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(UpdateOrbitSystemGroup))]
    [UpdateAfter(typeof(UpdateOrbitalPositionSystem))]
    public partial class StarfieldRotationSystem : SystemBase {
        protected override void OnUpdate() {
            OrbitalPosition ppos = GetComponent<OrbitalPosition>(
                GetSingletonEntity<PlayerOrbitTag>());
            LocalToWorld pltw = GetComponent<LocalToWorld>(
                GetSingletonEntity<PlayerTag>());
            
            Entities
                .WithAll<StarfieldTag>()
                .ForEach((ref TransformAspect transform) => {
                    var ltw = transform.LocalToWorld;
                    ltw.Rotation = ppos.LocalToWorld.Rotation;
                    ltw.Position = pltw.Position;
                    transform.LocalToWorld = ltw;
                    // transform.Position = pltw.Position;
                    // transform.LocalRotation = ppos.LocalToWorld.Rotation;
                })
                .Schedule();
        }
    }
}
