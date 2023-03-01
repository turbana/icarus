using System;

using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

using Icarus.UI;

namespace Icarus.Graphics {
    public enum TextUpdateFormat {
        Number12_0,
        Number9_2,
        Number6_5,
        Number1_10,
    }
    
    public struct TextUpdate : IBufferElementData {
        public FixedString64Bytes Key;
        public FixedString64Bytes Value;

        public TextUpdate(FixedString64Bytes key, FixedString64Bytes value) {
            Key = key;
            Value = value;
        }

        public TextUpdate(FixedString64Bytes key, object value) {
            Key = key;
            Value = value.ToString();
        }
        
        public TextUpdate(FixedString64Bytes key, double value, TextUpdateFormat fmt) {
            int left = -1;
            int right = -1;
            Key = key;
            Value = "";
            // fund formatting parameters
            switch (fmt) {
                case TextUpdateFormat.Number12_0: left=12; right=0;  break;
                case TextUpdateFormat.Number9_2:  left=9;  right=2;  break;
                case TextUpdateFormat.Number6_5:  left=6;  right=5;  break;
                case TextUpdateFormat.Number1_10: left=1;  right=10; break;
            }
            
            // size string
            int size = left + right;
            if (right > 0) size += 1; // add decimal place
            Value.Length = size + 1;
            Value[size] = 0x00;
            
            // format value
            int whole = (int)Math.Truncate(value);
            value -= whole;
            
            // insert decimal
            if (right > 0) Value[left] = 0x2E;
            
            // format whole number portion
            bool first = true;
            for (int i=left-1; i>=0; i--) {
                if (whole > 0 || first) {
                    int digit = whole % 10;
                    Value[i] = (byte)(0x30 + digit);
                    whole /= 10;
                    first = false;
                } else {
                    Value[i] = 0x20; // space
                }
            }
            // format digits
            for (int i=left+1; i<size; i++) {
                value *= 10;
                int digit = (int)value % 10;
                Value[i] = (byte)(0x30 + digit);
                value -= digit;
            }
        }
    }

    public struct ListenerUpdate : IBufferElementData {
        public FixedString64Bytes Key;
        public Entity Listener;
    }

    public class TextUpdateSystemAuthoring : MonoBehaviour {
        public class TextUpdateSystemAuthoringBaker : Baker<TextUpdateSystemAuthoring> {
            public override void Bake(TextUpdateSystemAuthoring auth) {
                AddBuffer<TextUpdate>();
                AddBuffer<ListenerUpdate>();
            }
        }
    }

    [BurstCompile]
    [UpdateInGroup(typeof(IcarusInteractionSystemGroup))]
    public partial class TextUpdateSystem : SystemBase {
        protected NativeHashMap<FixedString64Bytes, FixedString64Bytes> TextValues;
        protected NativeHashMap<FixedString64Bytes, UnsafeList<Entity>> Listeners;
        protected NativeHashSet<FixedString64Bytes> Changed;
        protected ComponentLookup<DisplayText> DisplayTextLookup;

        [BurstCompile]
        protected override void OnCreate() {
            TextValues = new NativeHashMap<FixedString64Bytes, FixedString64Bytes>(100, Allocator.Persistent);
            Listeners = new NativeHashMap<FixedString64Bytes, UnsafeList<Entity>>(100, Allocator.Persistent);
            Changed = new NativeHashSet<FixedString64Bytes>(100, Allocator.Persistent);
            DisplayTextLookup = SystemAPI.GetComponentLookup<DisplayText>(false);
        }
        
        [BurstCompile]
        protected override void OnUpdate() {
            DisplayTextLookup.Update(this);
            
            var updates = SystemAPI.GetSingletonBuffer<TextUpdate>(false);
            var newListeners = SystemAPI.GetSingletonBuffer<ListenerUpdate>(false);

            // add new listeners
            for (int i=0; i<newListeners.Length; i++) {
                var listener = newListeners[i];
                UnsafeList<Entity> listeners;
                if (!Listeners.TryGetValue(listener.Key, out listeners)) {
                    listeners = new UnsafeList<Entity>(1, Allocator.Persistent);
                }
                // UnityEngine.Debug.Log($"added listener for [{listener.Key}]");
                listeners.Add(in listener.Listener);
                Listeners[listener.Key] = listeners;
                // assume listener's text is changed so it gets updated at least once
                Changed.Add(listener.Key);
            }
            
            // collate all changes
            for (int i=0; i<updates.Length; i++) {
                var update = updates[i];
                if (!TextValues.ContainsKey(update.Key) || update.Value != TextValues[update.Key]) {
                    // UnityEngine.Debug.Log($"setting changed [{update.Key}]");
                    TextValues[update.Key] = update.Value;
                    Changed.Add(update.Key);
                }
            }
            
            // notify Entities of any changes
            var changed = Changed.ToNativeArray(Allocator.TempJob);
            for (int i=0; i<changed.Length; i++) {
                var key = changed[i];
                UnsafeList<Entity> entities;
                if (Listeners.TryGetValue(key, out entities)) {
                    // UnityEngine.Debug.Log($"{entities.Length} listeners found for [{key}]");
                    FixedString64Bytes value = TextValues[key];
                    foreach (Entity entity in entities) {
                        var comp = DisplayTextLookup[entity];
                        comp.Value = value;
                        DisplayTextLookup[entity] = comp;
                        // UnityEngine.Debug.Log($"updated entity with [{key}, {value}]");
                    }
                } else {
                    // UnityEngine.Debug.Log($"no listeners found for [{key}]");
                }
            }
            
            // clean up
            changed.Dispose();
            Changed.Clear();
            updates.Clear();
            newListeners.Clear();
        }
    }
}
