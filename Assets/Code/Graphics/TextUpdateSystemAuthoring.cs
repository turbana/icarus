using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace Icarus.Graphics {
    public partial class TextUpdateSystem : SystemBase {
        private struct TextUpdate {
            public FixedString64Bytes Key;
            public FixedString64Bytes Value;
        }
    
        private static NativeQueue<TextUpdate> UpdateQueue;
        private static NativeQueue<TextUpdate>.ParallelWriter UpdateQueueWriter;
        private static NativeHashMap<FixedString64Bytes, FixedString64Bytes> TextValues;
        private static NativeHashMap<FixedString64Bytes, UnsafeList<Entity>> Listeners;
        private static NativeHashSet<FixedString64Bytes> Changed;
        private static ComponentLookup<DisplayText> DisplayTextLookup;

        protected override void OnCreate() {
            UpdateQueue = new NativeQueue<TextUpdate>(Allocator.Persistent);
            UpdateQueueWriter = UpdateQueue.AsParallelWriter();
            TextValues = new NativeHashMap<FixedString64Bytes, FixedString64Bytes>(100, Allocator.Persistent);
            Listeners = new NativeHashMap<FixedString64Bytes, UnsafeList<Entity>>(100, Allocator.Persistent);
            Changed = new NativeHashSet<FixedString64Bytes>(100, Allocator.Persistent);
            DisplayTextLookup = SystemAPI.GetComponentLookup<DisplayText>(false);
        }
        
        protected override void OnUpdate() {
            DisplayTextLookup.Update(this);
            var updates = UpdateQueue.ToArray(Allocator.TempJob);
            
            // collate all changes
            foreach (TextUpdate update in updates) {
                if (!TextValues.ContainsKey(update.Key) || update.Value != TextValues[update.Key]) {
                    // UnityEngine.Debug.Log($"setting changed [{update.Key}]");
                    TextValues[update.Key] = update.Value;
                    Changed.Add(update.Key);
                }
            }
            
            // notify Entities of any changes
            foreach (FixedString64Bytes key in Changed) {
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
            updates.Dispose();
            Changed.Clear();
            UpdateQueue.Clear();
        }

        public static void Update(FixedString64Bytes key, FixedString64Bytes value) {
            UpdateQueueWriter.Enqueue(new TextUpdate { Key=key, Value=value });
            // UnityEngine.Debug.Log($"enqueue [{key}, {value}] ({UpdateQueue.Count})");
        }

        public static void RegisterListener(FixedString64Bytes key, in Entity entity) {
            UnsafeList<Entity> listeners;
            if (!Listeners.TryGetValue(key, out listeners)) {
                listeners = new UnsafeList<Entity>(1, Allocator.Persistent);
            }
            listeners.Add(in entity);
            Listeners[key] = listeners;
            // UnityEngine.Debug.Log($"added listener for [{key}] ({Listeners[key].Length}) (({Listeners[key].IsCreated} - {Listeners[key].IsEmpty}))");
        }
    }
}
