using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

using Icarus.UI;

namespace Icarus.Controls {
    public partial struct BridgeJumpTargetKeypadTag : IComponentData {}
    
    [BurstCompile]
    public partial class BridgeJumpTargetKeypad : SystemBase {
        public ComponentLookup<DatumDouble> DatumDoubleLookup;
        public ComponentLookup<DatumString64> DatumStringLookup;

        private const int KEYPAD_LENGTH = 12;
        private const byte SPACE = (byte)' ';
        private const byte DECIMAL = (byte)'.';
        readonly private static Unicode.Rune SPACE_RUNE = (Unicode.Rune)' ';
        readonly private static Unicode.Rune DECIMAL_RUNE = (Unicode.Rune)'.';

        [BurstCompile]
        protected override void OnCreate() {
            DatumDoubleLookup = GetComponentLookup<DatumDouble>(true);
            DatumStringLookup = GetComponentLookup<DatumString64>(false);
        }
        
        [BurstCompile]
        protected override void OnUpdate() {
            DatumDoubleLookup.Update(this);
            DatumStringLookup.Update(this);
            var DDL = DatumDoubleLookup;
            var DSL = DatumStringLookup;
            
            Entities
                .WithReadOnly(DDL)
                .WithAll<BridgeJumpTargetKeypadTag>()
                .ForEach((in DatumRefBufferCollection index, in DynamicBuffer<DatumRefBuffer> buffers) => {
                    var oentity = buffers[index["Bridge.JumpTarget.RegisterValue"]].Entity;
                    var output = DSL[oentity];

                    // ensure we're always working with a string of length 12
                    if (output.Value.Length == 0) {
                        output.Value.Append(SPACE_RUNE, KEYPAD_LENGTH);
                    }
                    
                    foreach (var pair in index.IndexMap) {
                        var id = pair.Key;
                        var idx = pair.Value;
                        var cut = id.LastIndexOf(DECIMAL_RUNE);
                        if (cut < 0) continue;
                        var prefix = id.Substring(0, cut);
                        if (prefix == "Bridge.JumpTarget.Keypad") {
                            var entity = buffers[idx].Entity;
                            var datum = DDL[entity];
                            if (datum.Dirty && datum.Value == 1) {
                                var suffix = id.Substring(cut + 1);
                                // UnityEngine.Debug.Log($"pressed {suffix}");
                                if (suffix == "<") {
                                    for (int i=KEYPAD_LENGTH-1; i>0; i--) {
                                        output.Value[i] = output.Value[i-1];
                                    }
                                    output.Value[0] = SPACE;
                                } else if (suffix == "Decimal") {
                                    var i = output.Value.IndexOf(DECIMAL_RUNE);
                                    if (i < 0) i = 0;
                                    for (; i<KEYPAD_LENGTH-1; i++) {
                                        output.Value[i] = output.Value[i+1];
                                    }
                                    output.Value[KEYPAD_LENGTH-1] = DECIMAL;
                                } else {
                                    for (int i=0; i<KEYPAD_LENGTH-1; i++) {
                                        output.Value[i] = output.Value[i+1];
                                    }
                                    output.Value[KEYPAD_LENGTH-1] = suffix[0];
                                }
                            }
                        }
                    }

                    if (output.Dirty) {
                        DSL[oentity] = output;
                    }
                })
                .Schedule();
        }
    }
}
