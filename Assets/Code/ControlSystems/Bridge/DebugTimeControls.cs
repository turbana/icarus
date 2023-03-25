using Unity.Burst;
using Unity.Entities;

using Icarus.Orbit;
using Icarus.UI;

namespace Icarus.Controls {
    public partial struct DebugTimeControlsTag : IComponentData {}

    [BurstCompile]
    [UpdateInGroup(typeof(IcarusInteractionSystemGroup))]
    public partial class DebugTimeControlsSystem : SystemBase {
        public ComponentLookup<DatumDouble> DatumDoubleLookup;
        public ComponentLookup<OrbitalOptions> OrbitalOptionsLookup;

        [BurstCompile]
        protected override void OnCreate() {
            DatumDoubleLookup = GetComponentLookup<DatumDouble>(false);
            OrbitalOptionsLookup = GetComponentLookup<OrbitalOptions>(false);
        }

        [BurstCompile]
        protected override void OnUpdate() {
            DatumDoubleLookup.Update(this);
            OrbitalOptionsLookup.Update(this);
            var DDL = DatumDoubleLookup;
            var OOL = OrbitalOptionsLookup;
            var ooEntity = SystemAPI.GetSingletonEntity<OrbitalOptions>();

            Entities
                .WithAll<DebugTimeControlsTag>()
                .ForEach((in DatumRefBufferCollection index,
                          in DynamicBuffer<DatumRefBuffer> buffers) => {
                             // UnityEngine.Debug.Log("1");
                    var timeEntity = buffers[index["World.TimeScale"]].Entity;
                    var timeDatum = DDL[timeEntity];
                             // UnityEngine.Debug.Log("2");
                    var increase = DDL[buffers[index["Debug.TimeControl.Increase"]].Entity];
                             // UnityEngine.Debug.Log("3");
                    var decrease = DDL[buffers[index["Debug.TimeControl.Decrease"]].Entity];
                             // UnityEngine.Debug.Log("4");
                    var reset = DDL[buffers[index["Debug.TimeControl.Reset"]].Entity];
                             // UnityEngine.Debug.Log("5 ");

                    if (reset.Dirty && reset.Value == 1) {
                        timeDatum.Value = 1f;
                        // UnityEngine.Debug.Log($"reset");
                    } else if (increase.Dirty && increase.Value == 1) {
                        timeDatum.Value = timeDatum.Value * 2f;
                        // UnityEngine.Debug.Log($"increase");
                    } else if (decrease.Dirty && decrease.Value == 1) {
                        timeDatum.Value = timeDatum.Value / 2f;
                        // UnityEngine.Debug.Log($"decrease");
                    }

                    if (timeDatum.Dirty) {
                        // UnityEngine.Debug.Log($"time = {timeDatum.Value}");
                        if (timeDatum.Value == 0f) timeDatum.Value = 1f;
                        DDL[timeEntity] = timeDatum;
                        var oo = OOL[ooEntity];
                        oo.TimeScale = (float)timeDatum.Value;
                        OOL[ooEntity] = oo;
                    }
                })
                // .WithoutBurst()
                .Schedule();
        }
    }
}
