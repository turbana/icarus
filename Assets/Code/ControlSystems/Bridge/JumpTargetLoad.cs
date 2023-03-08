using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

using Icarus.UI;

namespace Icarus.Controls {
    public partial struct BridgeJumpTargetLoadTag : IComponentData {}
    
    [BurstCompile]
    public partial class BridgeJumpTargetLoad : SystemBase {
        public ComponentLookup<DatumDouble> DatumDoubleLookup;
        public ComponentLookup<DatumString64> DatumStringLookup;

        private readonly static FixedString64Bytes[] TARGET_IDS = new FixedString64Bytes[] {
            "<ignore>",
            "Planned.Orbit.SemiMajorAxis",
            "Planned.Orbit.Period",
            "Planned.Orbit.Inclination",
            "Planned.Orbit.Eccentricity",
            "Planned.Orbit.AscendingNode",
        };

        [BurstCompile]
        protected override void OnCreate() {
            DatumDoubleLookup = GetComponentLookup<DatumDouble>(false);
            DatumStringLookup = GetComponentLookup<DatumString64>(true);
        }
        
        [BurstCompile]
        protected override void OnUpdate() {
            DatumDoubleLookup.Update(this);
            DatumStringLookup.Update(this);
            var DDL = DatumDoubleLookup;
            var DSL = DatumStringLookup;
            
            Entities
                .WithReadOnly(DSL)
                .WithAll<BridgeJumpTargetLoadTag>()
                .ForEach((in DatumRefBufferCollection index, in DynamicBuffer<DatumRefBuffer> buffers) => {
                    var button = buffers[index["Bridge.JumpTarget.Load"]].Entity;
                    var datum = DDL[button];

                    if (datum.Dirty && datum.Value == 1) {
                        var dial = buffers[index["Bridge.JumpTarget.Selector"]].Entity;
                        var position = (int)DDL[dial].Value;
                        // ignore first dial position
                        if (position > 0) {
                            var key = TARGET_IDS[position];
                            var keypad = buffers[index["Bridge.JumpTarget.Value"]].Entity;
                            var entity = buffers[index[key]].Entity;
                            var output = DDL[entity];
                            output.Value = DSL[keypad].DoubleValue;
                            DDL[entity] = output;
                        }
                    }
                })
                .Schedule();
        }
    }
}
