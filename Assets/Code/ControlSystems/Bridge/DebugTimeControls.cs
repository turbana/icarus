using Unity.Burst;
using Unity.Entities;

using Icarus.Orbit;
using Icarus.UI;

namespace Icarus.Controls {
    public partial struct DebugTimeControlsTag : IComponentData {}

    [BurstCompile]
    [UpdateInGroup(typeof(IcarusInteractionSystemGroup))]
    public partial class DebugTimeControlsSystem : SystemBase {
        public ComponentLookup<OrbitalOptions> OrbitalOptionsLookup;

        [BurstCompile]
        protected override void OnCreate() {
            OrbitalOptionsLookup = GetComponentLookup<OrbitalOptions>(false);
            RequireForUpdate<OrbitalOptions>();
        }

        [BurstCompile]
        protected override void OnUpdate() {
            OrbitalOptionsLookup.Update(this);
            var OOL = OrbitalOptionsLookup;
            var ooEntity = SystemAPI.GetSingletonEntity<OrbitalOptions>();

            Entities
                .WithAll<DebugTimeControlsTag>()
                .ForEach((ref DatumCollection datums) => {
                    var time = datums.GetDouble("World.TimeScale");
                    
                    if (datums.IsPressed("Debug.TimeControl.Reset")) {
                        time = 1;
                    } else if (datums.IsPressed("Debug.TimeControl.Increase")) {
                        time *= 2;
                    } else if (datums.IsPressed("Debug.TimeControl.Decrease")) {
                        time /= 2;
                    } else {
                        return;
                    }

                    datums.SetDouble("World.TimeScale", time);
                    var oo = OOL[ooEntity];
                    oo.TimeScale = (float)time;
                    OOL[ooEntity] = oo;
                })
                .Schedule();
        }
    }
}
