using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

using Icarus.UI;

namespace Icarus.Controls {
    public partial struct BridgeJumpTargetKeyboardTag : IComponentData {}
    
    [BurstCompile]
    public partial class BridgeJumpTargetKeyboard : SystemBase {
        public ComponentLookup<DatumDouble> DatumDoubleLookup;
        public ComponentLookup<DatumString512> DatumStringLookup;

        private static NativeArray<FixedString32Bytes> _values;
        
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

            // crt is 11 lines, minus 1 for the title, so 10 lines including input
            _values = new NativeArray<FixedString32Bytes>(10, Allocator.Persistent);
            _values[0] = "foobar";
            _values[1] = "Earth";
            _values[2] = "Saturn";
        }
        
        [BurstCompile]
        protected override void OnUpdate() {
            DatumDoubleLookup.Update(this);
            DatumStringLookup.Update(this);
            var DDL = DatumDoubleLookup;
            var DSL = DatumStringLookup;
            var values = _values;
            var cursor = (int)World.Time.ElapsedTime % 2 == 0;
            var prevCursor = (int)(World.Time.ElapsedTime - World.Time.DeltaTime) % 2 == 0;
            
            Entities
                .WithReadOnly(DDL)
                .WithAll<BridgeJumpTargetKeyboardTag>()
                .ForEach((in DatumRefBufferCollection index, in DynamicBuffer<DatumRefBuffer> buffers) => {
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
                                UnityEngine.Debug.Log($"pressed {suffix}");
                                dirty = true;
                                // TODO handle input
                            }
                        }
                    }

                    if (dirty || (cursor != prevCursor) || output.Value.Length == 0) {
                        BuildOutput(ref output.Value, values, cursor);
                        DSL[oentity] = output;
                    }
                })
                .Schedule();
        }

        [BurstCompile]
        private static void BuildOutput(ref FixedString512Bytes output, in NativeArray<FixedString32Bytes> values, bool showCursor) {
            var cursor = (showCursor ? '|' : ' ');
            output = string.Format("         JUMP TARGET\n> <u>{0}</u>{1}", values[0], cursor);
            for (int i=1; i<values.Length; i++) {
                var value = values[i];
                if (value == "") continue;
                if (value.Length > LINE_WIDTH) value = value.Substring(0, LINE_WIDTH);
                char c = (char)('a' + (i-1));
                output = string.Format("{0}\n<u>{1}</u> {2}", output, c, value);
            }
        }
    }
}
