using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

using Icarus.Orbit;
using Icarus.UI;

namespace Icarus.Controls {
    public partial struct BridgeJumpTargetKeyboardTag : IComponentData {}

    public partial struct BridgeJumpTargetValue : IBufferElementData {
        public FixedString32Bytes Value;
    }
    
    [BurstCompile]
    [UpdateInGroup(typeof(IcarusInteractionSystemGroup))]
    public partial class BridgeJumpTargetKeyboard : SystemBase {
        public ComponentLookup<DatumDouble> DatumDoubleLookup;
        public ComponentLookup<DatumString512> DatumStringLookup;
        public ComponentLookup<DatumString64> DatumString64Lookup;
        public ComponentLookup<OrbitalDatabaseComponent> DatabaseLookup;

        // line width is 30 characters minus 2 for the prefix character and space
        private const int INPUT_LENGTH = 28;
        private const int LINE_WIDTH = 28;
        private const int RESULT_LINES = 9;
        
        private const byte SPACE = (byte)' ';
        private const byte DECIMAL = (byte)'.';
        readonly private static Unicode.Rune SPACE_RUNE = (Unicode.Rune)' ';
        readonly private static Unicode.Rune DECIMAL_RUNE = (Unicode.Rune)'.';
        readonly private static Unicode.Rune A_RUNE = (Unicode.Rune)'a';

        [BurstCompile]
        protected override void OnCreate() {
            DatumDoubleLookup = GetComponentLookup<DatumDouble>(true);
            DatumStringLookup = GetComponentLookup<DatumString512>(false);
            DatumString64Lookup = GetComponentLookup<DatumString64>(false);
            DatabaseLookup = GetComponentLookup<OrbitalDatabaseComponent>(true);
        }
        
        [BurstCompile]
        protected override void OnUpdate() {
            DatumDoubleLookup.Update(this);
            DatumStringLookup.Update(this);
            DatumString64Lookup.Update(this);
            DatabaseLookup.Update(this);
            var DDL = DatumDoubleLookup;
            var DSL = DatumStringLookup;
            var DS64L = DatumString64Lookup;
            var DBL = DatabaseLookup;
            var cursor = (int)World.Time.ElapsedTime % 2 == 0;
            var prevCursor = (int)(World.Time.ElapsedTime - World.Time.DeltaTime) % 2 == 0;
            var dbEntity = SystemAPI.GetSingletonEntity<OrbitalDatabaseComponent>();
            
            Entities
                .WithReadOnly(DDL)
                .WithReadOnly(DBL)
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
                                var chosen = HandleInput(ref search, suffix);
                                if (0 < chosen && values[chosen].Value != "") {
                                    var target = buffers[index["Planned.Orbit.Target"]].Entity;
                                    var tmp = DS64L[target];
                                    tmp.Value = values[chosen].Value;
                                    DS64L[target] = tmp;
                                }

                                // did we change the search value?
                                if (search != values[0].Value) {
                                    dirty = true;
                                    // update search value
                                    var tmp = values[0];
                                    tmp.Value = search;
                                    values[0] = tmp;
                                    // perform search
                                    var db = DBL[dbEntity];
                                    var results = db.Search(search, RESULT_LINES, Allocator.TempJob);
                                    // UnityEngine.Debug.Log($"got {results.Length} results for {search}");
                                    for (int i=0; i<RESULT_LINES; i++) {
                                        var result = (i < results.Length && search != "") ? results[i] : "";
                                        tmp = values[i + 1];
                                        tmp.Value = new FixedString32Bytes(result.Substring(0, 29));
                                        values[i + 1] = tmp;
                                    }
                                    results.Dispose();
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
                // UnityEngine.Debug.Log($"{i} = ({value})");
                char c = (char)('a' + (i-1));
                output = string.Format("{0}\n<u>{1}</u> {2}", output, c, value);
            }
        }

        [BurstCompile]
        private static int HandleInput(ref FixedString32Bytes search, in FixedString64Bytes suffix) {
            // backspace
            if (suffix == "BKS") {
                if (search.Length > 0) {
                    search.Length -= 1;
                }
            }
            // selection keys
            else if (suffix == "a") return 1;
            else if (suffix == "b") return 2;
            else if (suffix == "c") return 3;
            else if (suffix == "d") return 4;
            else if (suffix == "e") return 5;
            else if (suffix == "f") return 6;
            else if (suffix == "g") return 7;
            else if (suffix == "h") return 8;
            else if (suffix == "i") return 9;
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

            return 0;
        }
    }
}
