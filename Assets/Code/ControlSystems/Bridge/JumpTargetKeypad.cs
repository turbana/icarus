using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

using Icarus.UI;

namespace Icarus.Controls {
    public partial struct BridgeJumpTargetKeypadTag : IComponentData {}
    
    [BurstCompile]
    [UpdateInGroup(typeof(IcarusInteractionSystemGroup))]
    public partial class BridgeJumpTargetKeypad : SystemBase {
        private const int KEYPAD_LENGTH = 12;
        private const byte SPACE = (byte)' ';
        private const byte DECIMAL = (byte)'.';
        readonly private static Unicode.Rune SPACE_RUNE = (Unicode.Rune)' ';
        readonly private static Unicode.Rune DECIMAL_RUNE = (Unicode.Rune)'.';

        [BurstCompile]
        protected override void OnCreate() {
            RequireForUpdate<DatumCollection>();
        }
        
        [BurstCompile]
        protected override void OnUpdate() {
            Entities
                .WithAll<BridgeJumpTargetKeypadTag>()
                .ForEach((ref DatumCollection datums) => {
                    FixedString64Bytes value =
                        (datums.HasDatum("Bridge.JumpTarget.RegisterValue"))
                        ? datums.GetString64("Bridge.JumpTarget.RegisterValue")
                        : "";
                    
                    if (value.Length < KEYPAD_LENGTH) {
                        value.Append(SPACE_RUNE, KEYPAD_LENGTH - value.Length);
                    }

                    var keys = datums.DoubleStartsWith("Bridge.JumpTarget.Keypad", Allocator.TempJob);
                    for (int j=0; j<keys.Length; j++) {
                        var key = keys[j];
                        if (datums.IsPressed(key)) {
                            var cut = key.LastIndexOf(DECIMAL_RUNE);
                            var suffix = key.Substring(cut + 1);
                            // UnityEngine.Debug.Log($"pressed {suffix} [{value}]");
                            if (suffix == "<") {
                                for (int i=KEYPAD_LENGTH-1; i>0; i--) {
                                    value[i] = value[i-1];
                                }
                                value[0] = SPACE;
                            } else if (suffix == "Decimal") {
                                var i = value.IndexOf(DECIMAL_RUNE);
                                if (i < 0) i = 0;
                                for (; i<KEYPAD_LENGTH-1; i++) {
                                    value[i] = value[i+1];
                                }
                                value[KEYPAD_LENGTH-1] = DECIMAL;
                            } else {
                                for (int i=0; i<KEYPAD_LENGTH-1; i++) {
                                    value[i] = value[i+1];
                                }
                                value[KEYPAD_LENGTH-1] = suffix[0];
                            }
                        }
                    }

                    keys.Dispose();
                    datums.SetString64("Bridge.JumpTarget.RegisterValue", value);
                })
                .Schedule();
        }
    }
}
