using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

using Icarus.UI;

namespace Icarus.Controls {
    public partial struct BridgeJumpTargetKeyboardTag : IComponentData {}

    public partial struct BridgeJumpTargetValue : IBufferElementData {
        public FixedString32Bytes Value;
    }
    
    [BurstCompile]
    public partial class BridgeJumpTargetKeyboard : SystemBase {
        public ComponentLookup<DatumDouble> DatumDoubleLookup;
        public ComponentLookup<DatumString512> DatumStringLookup;

        // line width is 30 characters minus 2 for the prefix character and space
        private const int INPUT_LENGTH = 28;
        private const int LINE_WIDTH = 28;
        
        private const byte SPACE = (byte)' ';
        private const byte DECIMAL = (byte)'.';
        readonly private static Unicode.Rune SPACE_RUNE = (Unicode.Rune)' ';
        readonly private static Unicode.Rune DECIMAL_RUNE = (Unicode.Rune)'.';
        readonly private static Unicode.Rune A_RUNE = (Unicode.Rune)'a';

        [BurstCompile]
        protected override void OnCreate() {
            DatumDoubleLookup = GetComponentLookup<DatumDouble>(true);
            DatumStringLookup = GetComponentLookup<DatumString512>(false);
        }
        
        [BurstCompile]
        protected override void OnUpdate() {
            DatumDoubleLookup.Update(this);
            DatumStringLookup.Update(this);
            var DDL = DatumDoubleLookup;
            var DSL = DatumStringLookup;
            var cursor = (int)World.Time.ElapsedTime % 2 == 0;
            var prevCursor = (int)(World.Time.ElapsedTime - World.Time.DeltaTime) % 2 == 0;
            
            Entities
                .WithReadOnly(DDL)
                .WithAll<BridgeJumpTargetKeyboardTag>()
                .ForEach((ref DynamicBuffer<BridgeJumpTargetValue> values,
                          in DatumRefBufferCollection index,
                          in DynamicBuffer<DatumRefBuffer> buffers
                         ) => {
                    var oentity = buffers[index["Bridge.JumpTarget.Computer"]].Entity;
                    var output = DSL[oentity];
                    var dirty = false;
                    
                    foreach (var pair in index.IndexMap) {
                        var id = pair.Key;
                        var idx = pair.Value;
                        var cut = id.LastIndexOf(DECIMAL_RUNE);
                        if (cut < 0) continue;
                        var prefix = id.Substring(0, cut);
                        if (prefix == "Bridge.JumpTarget.Keyboard") {
                            var entity = buffers[idx].Entity;
                            if (!DDL.HasComponent(entity)) continue;
                            var datum = DDL[entity];
                            if (datum.Dirty && datum.Value == 1) {
                                var suffix = id.Substring(cut + 1);
                                var search = values[0].Value;
                                // UnityEngine.Debug.Log($"pressed {suffix}");
                                HandleInput(ref search, suffix);

                                // did we change the search value?
                                if (search != values[0].Value) {
                                    dirty = true;
                                    // update search value
                                    var tmp = values[0];
                                    tmp.Value = search;
                                    values[0] = tmp;
                                    // TODO perform search
                                }
                            }
                        }
                    }

                    // refresh the screen when either:
                    // - we've updated the search term
                    // - we need to toggle the cursor visibility
                    // - or the first time the computer is being displayed
                    if (dirty || (cursor != prevCursor) || output.Value.Length == 0) {
                        BuildOutput(ref output.Value, values, cursor);
                        DSL[oentity] = output;
                    }
                })
                .Schedule();
        }

        [BurstCompile]
        private static void BuildOutput(ref FixedString512Bytes output, in DynamicBuffer<BridgeJumpTargetValue> buffer, bool showCursor) {
            var cursor = (showCursor ? '|' : ' ');
            output = string.Format("         JUMP TARGET\n> <u>{0}</u>{1}", buffer[0].Value, cursor);
            for (int i=1; i<buffer.Length; i++) {
                var value = buffer[i].Value;
                if (value.IsEmpty) continue;
                if (value.Length > LINE_WIDTH) value = value.Substring(0, LINE_WIDTH);
                UnityEngine.Debug.Log($"({value})");
                char c = (char)('a' + (i-1));
                output = string.Format("{0}\n<u>{1}</u> {2}", output, c, value);
            }
        }

        [BurstCompile]
        private static void HandleInput(ref FixedString32Bytes search, in FixedString64Bytes suffix) {
            // backspace
            if (suffix == "BKS") {
                if (search.Length > 0) {
                    search.Length -= 1;
                }
            }
            // selection keys
            else if (suffix == "a" ||
                     suffix == "b" ||
                     suffix == "c" ||
                     suffix == "d" ||
                     suffix == "e" ||
                     suffix == "f" ||
                     suffix == "g" ||
                     suffix == "h" ||
                     suffix == "i") {
                UnityEngine.Debug.Log("selection key");
            }
            // space key
            else if (suffix == "Space") {
                if (search.Length < INPUT_LENGTH) {
                    search.Add((byte)' ');
                }
            }
            // ignore keys
            else if (suffix == "RET" ||
                     suffix == "Up" ||
                     suffix == "Down" ||
                     suffix == "Left" ||
                     suffix == "Right" ||
                     suffix == "unknown1" ||
                     suffix == "unknown2" ||
                     suffix == "unknown3" ||
                     suffix == "unknown4" ||
                     suffix == "unknown5"
            ) { /* ignore */ }
            // all other keys
            else {
                if (search.Length < INPUT_LENGTH) {
                    search.Append(suffix);
                }
            }
        }
    }
}
