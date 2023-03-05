using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

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
        Int = 0, Float, Double, Bool, Byte, String32
    }

    /* A DatumRef holds a reference to a Datum. This consists of an DatumType
     * that the Datum is and and Entity that holds the Datum. */
    public partial struct DatumRef : IComponentData {
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

    /* A DatumBackRef holds an Entity reference to the Entity that references
     * the Datum. */
    public partial struct DatumBackRef : IBufferElementData {
        public Entity Value;
    }

    /* All Datums must implement Datum. This helps to find all Datums. */
    public partial interface IDatum : IComponentData {}
    
    /*** The Datums definitions ***/
    public partial struct DatumInt : IDatum { public int Value; public int PreviousValue; }
    public partial struct DatumFloat : IDatum { public float Value; public float PreviousValue; }
    public partial struct DatumDouble : IDatum { public double Value; public double PreviousValue; }
    public partial struct DatumBool : IDatum { public bool Value; public bool PreviousValue; }
    public partial struct DatumByte : IDatum { public byte Value; public byte PreviousValue;}
    public partial struct DatumString32 : IDatum { public FixedString32Bytes Value; public FixedString32Bytes PreviousValue; }

    /* The DatumRegistry holds a mapping from IDs -> Entity for all Datum
     * types. */
    public partial struct DatumRegistry : IComponentData {
        public UnsafeHashMap<FixedString64Bytes, Entity> Map;
        public UnsafeHashMap<Entity, DynamicBuffer<DatumBackRef>> BackMap;

        [BurstCompile]
        public Entity Lookup(in FixedString64Bytes ID, bool errorCheck=true) {
            if (!Map.TryGetValue(ID, out Entity entity)) {
                if (errorCheck) {
                    throw new System.ArgumentException($"Datum not created for id={ID}");
                }
                entity = Entity.Null;
            }
            return entity;
        }

        [BurstCompile]
        public Entity Lookup(ref EntityCommandBuffer ecb, in UninitializedDatumRef datum) {
            var entity = Lookup(in datum.ID, false);
            if (entity == Entity.Null) {
                entity = ecb.CreateEntity();
                switch (datum.Type) {
                    case DatumType.Int: ecb.AddComponent<DatumInt>(entity); break;
                    case DatumType.Float: ecb.AddComponent<DatumFloat>(entity); break;
                    case DatumType.Double: ecb.AddComponent<DatumDouble>(entity); break;
                    case DatumType.Bool: ecb.AddComponent<DatumBool>(entity); break;
                    case DatumType.Byte: ecb.AddComponent<DatumByte>(entity); break;
                    case DatumType.String32: ecb.AddComponent<DatumString32>(entity); break;
                }
                Map[datum.ID] = entity;
            }
            return entity;
        }

        [BurstCompile]
        public void AddBackRef(ref EntityCommandBuffer ecb, in FixedString64Bytes ID, in Entity entity) {
            var dentity = Lookup(in ID);
            if (!BackMap.TryGetValue(dentity, out DynamicBuffer<DatumBackRef> buffer)) {
                buffer = ecb.AddBuffer<DatumBackRef>(dentity);
                BackMap[dentity] = buffer;
            }
            buffer.Add(new DatumBackRef { Value = entity });
        }
    }
}
