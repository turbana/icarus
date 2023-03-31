using System;
using System.Threading;

using UnityEngine;
using UnityEngine.Rendering;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using TMPro;

// [assembly:RegisterGenericComponentType(typeof(Icarus.UI.DatumByte))]
// [assembly:RegisterGenericComponentType(typeof(Icarus.UI.Datum<byte>))]

namespace Icarus.UI {
    /* InteractionType is an index into the bit field Interaction.Mask */
    public enum InteractionType : uint {
        LeftMouse = 0,          // state of left mouse button
        LeftMouseDown,          // 1 on the frame left mouse button is first pressed
        LeftMouseUp,            // 1 on the frame left mouse button is released
        ScrollWheelUp,          // 1 on the frame mouse scroll wheel is up
        ScrollWheelDown,        // 1 on the frame mouse scroll wheel is down
        GiveControl,            // 1 on the frame the main interaction key is pressed
    }

    /* An Interaction represents a set of Interactions (either user
     * generated or a set a collider is willing to consume) */
    public partial struct Interaction : IComponentData {
        public BitField32 Value; // what interactions are currently occurring?
        public BitField32 Mask;  // what interactions will this consume

        public bool AnyInteraction { get => Value.Value != 0u; }
        public bool LeftMouse { get => Value.IsSet((int)InteractionType.LeftMouse); }
        public bool LeftMouseDown { get => Value.IsSet((int)InteractionType.LeftMouseDown); }
        public bool LeftMouseUp { get => Value.IsSet((int)InteractionType.LeftMouseUp); }
        public bool ScrollWheelUp { get => Value.IsSet((int)InteractionType.ScrollWheelUp); }
        public bool ScrollWheelDown { get => Value.IsSet((int)InteractionType.ScrollWheelDown); }
        public bool GiveControl { get => Value.IsSet((int)InteractionType.GiveControl); }

        public bool CanConsume(Interaction inputs) {
            return 0 != (Mask.Value & inputs.Value.Value);
        }

        public static Interaction FromUserInput() {
            var scroll = Input.mouseScrollDelta[1];
            var mask = (Input.GetMouseButton(0) ? 1<<(int)InteractionType.LeftMouse : 0)
                | (Input.GetMouseButtonDown(0) ? 1<<(int)InteractionType.LeftMouseDown : 0)
                | (Input.GetMouseButtonUp(0) ? 1<<(int)InteractionType.LeftMouseUp : 0)
                | (scroll > 0f ? 1<<(int)InteractionType.ScrollWheelUp : 0)
                | (scroll < 0f ? 1<<(int)InteractionType.ScrollWheelDown : 0)
                | (Input.GetKeyDown("e") ? 1<<(int)InteractionType.GiveControl : 0);
            return new Interaction() {
                Value = new BitField32() { Value = (uint)mask },
                Mask = default,
            };
        }

        public static Interaction FromMask(params InteractionType[] types) {
            int mask = 0;
            foreach (var type in types) {
                mask |= 1 << (int)type;
            }
            return new Interaction() {
                Value = default,
                Mask = new BitField32() { Value = (uint)mask },
            };
        }
    }

    /* InteractionControlType is used to signify a desired increase or decrease
     * in a control value. */
    public enum InteractionControlType { Increase, Decrease, Toggle, Press };

    /* ControlSettings contain various setting for a control. */
    public partial struct ControlSettings : IComponentData {
        public int Stops;       // number of "stops" on the dial/switch
        public float Rotation;  // amount of rotation for each stop (in radians)
        public float3 Movement; // amount of movement for each stop
        public InteractionControlType Type; // type of control
        public Entity Root;                 // the base entity of this control
        public LocalTransform InitialTransform; // the initial position of the control
    }

    /* Cross hair types */
    public enum CrosshairType : int {
        Normal = 0, Toggle, Increase, Decrease, Enter
    }

    /* Crosshair Component */
    public partial struct Crosshair : IComponentData {
        public CrosshairType Value;
    }

    /* Crosshair GameObject Component */
    public class CrosshairConfig : IComponentData {
        public GameObject GO;
        public Sprite[] Crosshairs;
    }

    /* A Datum is considered a single piece of data. All Datum are shared
     * between client and server. Clients will write to Datums through
     * interactions (buttons, switches, etc); the server will process updates
     * to them (keypads, keyboards, other higher level controls); the client
     * will render any changes to them through control outputs (displays,
     * gauges, leds, etc).
     *
     * A Datum can be of various types depending on need (float, int,
     * FixedStringXBytes, etc). Each value type needs it's own component. To
     * reference a Datum a DatumRef is required. It will contain both an Entity
     * reference and a type describing what Datum type is attached to the
     * Entity.
     */

    /* DatumType describes what type of Datum is attached to an Entity. */
    public enum DatumType : byte {
        Double = 0,
        String64,
        String512,
    }

    /* A DatumRef holds a reference to a Datum (Name and Type) */
    public partial struct DatumRef : IComponentData {
        public FixedString64Bytes Name;
        public DatumType Type;
    }

    /*** The Datums definitions ***/
    public partial struct DatumDouble : IComponentData {
        public FixedString64Bytes ID;
        public double Value;
        public double PreviousValue;

        public bool Dirty => Value != PreviousValue;
    }
    
    public partial struct DatumString64 : IComponentData {
        public FixedString64Bytes ID;
        public FixedString64Bytes Value;
        public FixedString64Bytes PreviousValue;
        
        public bool Dirty => Value != PreviousValue;

        public double DoubleValue => (double)FloatValue;
        public float FloatValue {
            get {
                int offset = 0;
                float result = 0f;
                // find first non-space character
                for (; offset<Value.Length; offset++) {
                    if (Value[offset] != ' ') break;
                }
                // assume empty string is 0
                if (offset == Value.Length) return 0f;
                var error = Value.Parse(ref offset, ref result);
                if (error != ParseError.None) {
                    throw new System.Exception($"could not parse string into float: {Value}");
                }
                return result;
            }
        }
    }

    public partial struct DatumString512 : IComponentData {
        public FixedString64Bytes ID;
        public FixedString512Bytes Value;
        public FixedString512Bytes PreviousValue;
        
        public bool Dirty => Value != PreviousValue;
    }

    /* A ManagedTextComponent holds a reference to a GameObject that contains a
     * TextMeshPro component. */
    public class ManagedTextComponent : IComponentData, IDisposable, ICloneable {
        public GameObject GO;
        public TextStyle Style;
        public string Format;

        public TextMeshPro TextMeshPro => this.GO.GetComponent<TextMeshPro>();
        public RectTransform RectTransform => this.GO.GetComponent<RectTransform>();

        public void Dispose() {
            #if UNITY_EDITOR
            UnityEngine.Object.DestroyImmediate(GO);
            #else
            UnityEngine.Object.Destroy(GO);
            #endif
        }

        public object Clone() {
            return new ManagedTextComponent {
                GO = (this.GO is null) ? null : UnityEngine.Object.Instantiate(this.GO),
                Style = this.Style,
                Format = this.Format,
            };
        }

        public void CreateGameObject() {
            if (this.GO is null) {
                this.GO = new GameObject("[Text]", typeof(RectTransform), typeof(MeshRenderer), typeof(TextMeshPro));
            }
            Init();
        }

        public void Init() {
            var tmp = this.TextMeshPro;
            var rt = this.RectTransform;
            var rend = this.GO.GetComponent<MeshRenderer>();
            // set common font settings
            rend.shadowCastingMode = ShadowCastingMode.Off;
            tmp.enableAutoSizing = false;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            // set custom font settings
            rt.sizeDelta = Style.Bounds;
            tmp.color = Style.FontColor;
            tmp.fontSize = Style.FontSize;
            tmp.fontStyle = Style.FontStyle;
            tmp.horizontalAlignment = Style.HAlign;
            tmp.verticalAlignment = Style.VAlign;
            tmp.font = Style.FontAsset;
            tmp.fontMaterial = Style.FontMaterial;
        }

        public void UpdateText(double value) {
            if (this.GO is null) this.CreateGameObject();
            this.TextMeshPro.text = String.Format(this.Format, value);
        }
        
        public void UpdateText(FixedString64Bytes value) {
            if (this.GO is null) this.CreateGameObject();
            this.TextMeshPro.text = String.Format(this.Format, value);
        }

        public void UpdateText(FixedString512Bytes value) {
            if (this.GO is null) this.CreateGameObject();
            this.TextMeshPro.text = String.Format(this.Format, value);
        }

        public void UpdatePosition(in LocalToWorld pos) =>
            UpdatePosition(pos.Position, pos.Rotation, new float3(1f));
        
        public void UpdatePosition(Vector3 pos, Quaternion rot, Vector3 scale) {
            if (this.GO is null) this.CreateGameObject();
            var rt = this.RectTransform;
            rt.position = pos;
            rt.rotation = rot * Quaternion.Euler(0f, -90f, 0f);
            rt.localScale = scale;
        }
    }

    /* create a shared static counter for use in threading DatumCollection. */
    public class DatumCollectionLock {
        public static readonly SharedStatic<int> Lock = SharedStatic<int>
            .GetOrCreate<DatumCollectionLock, LockKey>();
        private class LockKey {}

        // static DatumCollectionLock(int start, int end) {
        //     while (start == Interlocked.Exchange(ref Lock, end));
        // }
    }

    [BurstCompile]
    public unsafe partial struct DatumCollection : IComponentData {
        public UnsafeHashMap<FixedString64Bytes, DatumDouble> DatumDouble;
        public UnsafeHashMap<FixedString64Bytes, DatumString64> DatumString64;
        public UnsafeHashMap<FixedString64Bytes, DatumString512> DatumString512;
        public UnsafeHashSet<FixedString64Bytes> Dirty;
        public UnsafeHashSet<FixedString64Bytes> Keys;
        
        public DatumCollection(AllocatorManager.AllocatorHandle allocator) {
            DatumDouble = new UnsafeHashMap<FixedString64Bytes, DatumDouble>(1000, allocator);
            DatumString64 = new UnsafeHashMap<FixedString64Bytes, DatumString64>(1000, allocator);
            DatumString512 = new UnsafeHashMap<FixedString64Bytes, DatumString512>(100, allocator);
            Dirty = new UnsafeHashSet<FixedString64Bytes>(1000, allocator);
            Keys = new UnsafeHashSet<FixedString64Bytes>(1000, allocator);
        }

        [BurstCompile]
        public void Dispose() {
            DatumDouble.Dispose();
            DatumString64.Dispose();
            DatumString512.Dispose();
            Dirty.Dispose();
            Keys.Dispose();
        }

        /* threading */

        [NativeDisableUnsafePtrRestriction]
        internal static readonly SharedStatic<int> _lock = SharedStatic<int>
            .GetOrCreate<DatumCollection>();
        internal ref int _Lock => ref UnsafeUtility
            .AsRef<int>(UnsafeUtilityExtensions.AddressOf(_lock));
        
        [BurstCompile]
        private void _AquireLock() {
            while (Interlocked.Exchange(ref _Lock, 1) == 0);
        }

        [BurstCompile]
        private void _ReleaseLock() {
            Interlocked.Exchange(ref _Lock, 0);
        }

        /* public misc */
        
        [BurstCompile]
        public bool HasDatum(FixedString64Bytes name) {
            _AquireLock();
            var result = Keys.Contains(name);
            _ReleaseLock();
            return result;
        }

        [BurstCompile]
        public bool IsDirty(FixedString64Bytes name) {
            _AquireLock();
            _AssertKey(name);
            var result = Dirty.Contains(name);
            _ReleaseLock();
            return result;
        }

        [BurstCompile]
        public bool IsPressed(FixedString64Bytes name) {
            _AquireLock();
            // don't assert the key
            var result = Dirty.Contains(name) && DatumDouble[name].Value == 1;
            _ReleaseLock();
            return result;
        }

        [BurstCompile]
        public void ResetDirty() {
            _AquireLock();
            Dirty.Clear();
            _ReleaseLock();
        }

        /* private misc (without locking) */
        
        [BurstCompile]
        private bool _HasDatum(FixedString64Bytes name) {
            return Keys.Contains(name);
        }

        [BurstCompile]
        private void _AssertKey(FixedString64Bytes key) {
            // UnityEngine.Debug.Log($"_AssertKey({key})");
#if UNITY_EDITOR
            if (!_HasDatum(key)) {
                _ReleaseLock();
                throw new System.ArgumentException($"datum not set: {key}");
            }
#endif
        }

        [BurstCompile]
        private void _AddKey(FixedString64Bytes name) {
            // UnityEngine.Debug.Log($"adding key: {name} [{Keys.Count}/{Dirty.Count}] [{DatumDouble.Count}/{DatumString64.Count}/{DatumString512.Count}]");
            Keys.Add(name);
            Dirty.Add(name);
        }

        /* getters */

        [BurstCompile]
        public double GetDouble(FixedString64Bytes name) {
            _AquireLock();
            _AssertKey(name);
            var result = DatumDouble[name].Value;
            _ReleaseLock();
            return result;
        }

        [BurstCompile]
        public double GetDouble(FixedString64Bytes name, double defaultValue) {
            _AquireLock();
            var result = _HasDatum(name)
                ? DatumDouble[name].Value
                : defaultValue;
            _ReleaseLock();
            return result;
        }

        [BurstCompile]
        public FixedString64Bytes GetString64(FixedString64Bytes name) {
            _AquireLock();
            _AssertKey(name);
            var result = DatumString64[name].Value;
            _ReleaseLock();
            return result;
        }

        [BurstCompile]
        public FixedString64Bytes GetString64(FixedString64Bytes name, FixedString64Bytes defaultValue) {
            _AquireLock();
            var result = _HasDatum(name)
                ? DatumString64[name].Value
                : defaultValue;
            _ReleaseLock();
            return result;
        }

        [BurstCompile]
        public FixedString512Bytes GetString512(FixedString64Bytes name) {
            _AquireLock();
            _AssertKey(name);
            var result = DatumString512[name].Value;
            _ReleaseLock();
            return result;
        }

        [BurstCompile]
        public FixedString512Bytes GetString512(FixedString64Bytes name, FixedString512Bytes defaultValue) {
            _AquireLock();
            var result = _HasDatum(name)
                ? DatumString512[name].Value
                : defaultValue;
            _ReleaseLock();
            return result;
        }

        /* setters */

        [BurstCompile]
        public void SetDouble(FixedString64Bytes name, double value) {
            _AquireLock();
            DatumDouble datum;
            if (DatumDouble.ContainsKey(name)) {
                datum = DatumDouble[name];
                datum.Value = value;
            } else {
                datum = new DatumDouble { ID = name, Value = value };
            }
            _AddKey(name);
            DatumDouble[name] = datum;
            _ReleaseLock();
        }

        [BurstCompile]
        public void SetString64(FixedString64Bytes name, FixedString64Bytes value) {
            _AquireLock();
            DatumString64 datum;
            if (DatumString64.ContainsKey(name)) {
                datum = DatumString64[name];
                datum.Value = value;
            } else {
                datum = new DatumString64 { ID = name, Value = value };
            }
            _AddKey(name);
            DatumString64[name] = datum;
            _ReleaseLock();
        }

        [BurstCompile]
        public void SetString512(FixedString64Bytes name, FixedString512Bytes value) {
            _AquireLock();
            DatumString512 datum;
            if (DatumString512.ContainsKey(name)) {
                datum = DatumString512[name];
                datum.Value = value;
            } else {
                datum = new DatumString512 { ID = name, Value = value };
            }
            _AddKey(name);
            DatumString512[name] = datum;
            _ReleaseLock();
        }

        /* searchers */

        [BurstCompile]
        public UnsafeList<FixedString64Bytes> DoubleStartsWith(in FixedString64Bytes prefix, AllocatorManager.AllocatorHandle allocator) {
            var results = new UnsafeList<FixedString64Bytes>(10, allocator);
            _AquireLock();
            var keys = DatumDouble.GetKeyArray(Allocator.Temp);
            _ReleaseLock();
            for (int i=0; i<keys.Length; i++) {
                var key = keys[i];
                if (StringStartsWith(key, prefix)) {
                    results.Add(key);
                }
            }
            keys.Dispose();
            return results;
        }

        // this is defined in collections under an extension method, not sure
        // why I can't seem to use it.
        // NOTE: prefix must be at least as long as str.
        [BurstCompile]
        private static bool StringStartsWith(in FixedString64Bytes str, in FixedString64Bytes prefix) {
            for (int i=0; i<prefix.Length; i++) {
                if (str[i] != prefix[i]) return false;
            }
            return true;
        }
    }
}
