using System;

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
        Double = 0, String64
    }

    /* A DatumRef holds a reference to a Datum. This consists of an DatumType
     * that the Datum is and and Entity that holds the Datum. */
    public partial struct DatumRef : IComponentData {
        public Entity Entity;
        public DatumType Type;
    }

    /* A DatumRefBuffer can be used as a DynamicList<> for a list of Datum
     * references.*/
    public partial struct DatumRefBuffer : IBufferElementData {
        public Entity Entity;
        public DatumType Type;
    }

    /* An UninitializedDatumRef is a DatumRef that has yet to be connected to a
     * live Entity. This should be inserted at Bake time, and resolved at Load
     * time. */
    public partial struct UninitializedDatumRef : IComponentData {
        public FixedString64Bytes ID;
        public DatumType Type;
    }

    /* An UninitializedDatumRefBuffer can be used as a DynamicList<> for a list
     * of uninitialized Datum references. */
    public partial struct UninitializedDatumRefBuffer : IBufferElementData {
        public FixedString64Bytes ID;
        public DatumType Type;
    }

    /* A DatumRefBufferCollector is applied during baking to an Entity that
     * wants a DatumRefBufferCollection. A baking system runs to populate the
     * *Collection from the *Collector. */
    public partial struct DatumRefBufferCollector : IComponentData {
        public UnsafeList<Entity> Children;
        public UnsafeList<UninitializedDatumRefBuffer> ExtraBuffers;
    }

    /* A DatumRefBufferCollection holds ID/index information on the sibling
     * DynamicBuffer<DatumRefBuffer> component. This can be used to lookup
     * specific DatumRefBuffer by it's ID rather than index. */
    public partial struct DatumRefBufferCollection : IComponentData {
        public NativeHashMap<FixedString64Bytes, int> IndexMap;
    }
    
    // /*** The Datums definitions ***/
    public partial struct DatumDouble : IComponentData {
        public FixedString64Bytes ID;
        public double Value;
        public double PreviousValue;
    }
    
    public partial struct DatumString64 : IComponentData {
        public FixedString64Bytes ID;
        public FixedString64Bytes Value;
        public FixedString64Bytes PreviousValue;
    }

    /* A ManagedTextComponent holds a reference to a GameObject that contains a
     * TextMeshPro component. */
    public class ManagedTextComponent : IComponentData, IDisposable, ICloneable {
        public GameObject GO;
        public TMP_FontAsset Font;
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
                Font = this.Font,
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
            var config = TextStyleConfig.CONFIG[(int)Style];
            // set common font settings
            rend.shadowCastingMode = ShadowCastingMode.Off;
            tmp.enableAutoSizing = false;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.overflowMode = TextOverflowModes.Overflow;
            // set custom font settings
            rt.sizeDelta = config.Bounds;
            tmp.color = config.Color;
            tmp.fontSize = config.Size;
            tmp.fontStyle = config.Style;
            tmp.horizontalAlignment = config.HAlign;
            tmp.verticalAlignment = config.VAlign;
        }

        public void UpdateText(double value) {
            if (this.GO is null) this.CreateGameObject();
            this.TextMeshPro.text = String.Format(this.Format, value);
        }

        public void UpdatePosition(in TransformAspect pos) =>
            UpdatePosition(pos.WorldPosition, pos.WorldRotation, new float3(pos.WorldScale));
        
        public void UpdatePosition(Vector3 pos, Quaternion rot, Vector3 scale) {
            if (this.GO is null) this.CreateGameObject();
            var rt = this.RectTransform;
            rt.position = pos;
            rt.rotation = rot * Quaternion.Euler(0f, -90f, 0f);
            rt.localScale = scale;
        }
    }
}
