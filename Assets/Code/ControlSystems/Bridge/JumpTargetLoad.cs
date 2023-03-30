using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

using Icarus.UI;

namespace Icarus.Controls {
    public partial struct BridgeJumpTargetLoadTag : IComponentData {}
    
    [BurstCompile]
    [UpdateInGroup(typeof(IcarusInteractionSystemGroup))]
    public partial class BridgeJumpTargetLoad : SystemBase {
        private readonly static FixedString64Bytes[] TARGET_IDS = new FixedString64Bytes[] {
            "<ignore>",
            "Planned.Orbit.SemiMajorAxis",
            // "Planned.Orbit.Period",
            "Planned.Orbit.Inclination",
            "Planned.Orbit.Eccentricity",
            "Planned.Orbit.AscendingNode",
        };
        
        [BurstCompile]
        protected override void OnUpdate() {
            Entities
                .WithAll<BridgeJumpTargetLoadTag>()
                .ForEach((ref DatumCollection datums) => {
                    if (datums.IsPressed("Bridge.JumpTarget.Load")) {
                        var position = datums.GetDouble("Bridge.JumpTarget.Selector");
                        // ignore first dial position
                        if (position > 0) {
                            var register = datums.GetString64("Bridge.JumpTarget.RegisterValue");
                            var target = TARGET_IDS[(int)position];
                            // XXX we need to convert from double -> string
                            var datum = new DatumString64 { Value = register };
                            datums.SetDouble(target, datum.DoubleValue);
                        }
                    }
                })
                .Schedule();
        }
    }
}
