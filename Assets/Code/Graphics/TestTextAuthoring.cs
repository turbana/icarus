using System;

using UnityEngine;
using UnityEngine.Rendering;

using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using TMPro;

namespace Icarus.Graphics {
    public enum TextStyle {
        Display1
    };

    public class TextStyleConfig {
        public Vector2 Bounds;
        public float Size;
        public FontStyles Style;
        public Color Color;
        public HorizontalAlignmentOptions HAlign;
        public VerticalAlignmentOptions VAlign;

        public static TextStyleConfig[] CONFIG = new TextStyleConfig[] {
            // Display1
            new TextStyleConfig {
                Bounds = new Vector2(0.45f, 0.06f),
                Size = 0.6f,
                Style = FontStyles.Normal,
                Color = new Color(1f, 1f, 1f),
                HAlign = HorizontalAlignmentOptions.Center,
                VAlign = VerticalAlignmentOptions.Middle,
            },
        };
    }

    public struct DisplayText : IComponentData {
        public FixedString64Bytes Key;
        public FixedString64Bytes Value;
        public TextStyle Style;
    }
    
    public class ManagedTextComponent : IComponentData, IDisposable, ICloneable {
        public GameObject GO;
        public TMP_FontAsset Font;

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
                Font = this.Font
            };
        }
    }

    public class TestTextAuthoring : MonoBehaviour {
        public string key;
        public TextStyle style;
        public TMP_FontAsset font;
        
        public class TestTextAuthoringBaker : Baker<TestTextAuthoring> {
            public override void Bake(TestTextAuthoring auth) {
                AddComponent<DisplayText>(new DisplayText {
                        Key = new FixedString64Bytes(auth.key),
                        Value = new FixedString64Bytes(),
                        Style = auth.style,
                    });
                AddComponentObject<ManagedTextComponent>(new ManagedTextComponent {
                        GO = null,
                        Font = auth.font
                    });
            }
        }
    }

    public partial class UpdateTextObjectsSystem : SystemBase {
        protected override void OnUpdate() {
            Entities
                .WithChangeFilter<DisplayText>()
                .ForEach((Entity entity, ManagedTextComponent comp, in DisplayText text, in TransformAspect pos) => {
                    TextMeshPro tmp;
                    RectTransform rt;
                    if (comp.GO is null) {
                        comp.GO = new GameObject($"DisplayText[{text.Key}]", typeof(RectTransform), typeof(MeshRenderer), typeof(TextMeshPro));
                        tmp = comp.GO.GetComponent<TextMeshPro>();
                        rt = comp.GO.GetComponent<RectTransform>();
                        var rend = comp.GO.GetComponent<MeshRenderer>();
                        var config = TextStyleConfig.CONFIG[(int)text.Style];
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
                        // register listener
                        TextUpdateSystem.RegisterListener(text.Key, in entity);
                    } else {
                        tmp = comp.GO.GetComponent<TextMeshPro>();
                        rt = comp.GO.GetComponent<RectTransform>();
                    }
                    // update text
                    tmp.text = text.Value.ToString();
                    // update position / rotation / scale
                    rt.position = pos.WorldPosition;
                    rt.rotation = (Quaternion)pos.WorldRotation * Quaternion.Euler(0f, -90f, 0f);
                    rt.localScale = new Vector3(pos.WorldScale, pos.WorldScale, pos.WorldScale);
                    })
                .WithoutBurst()
                .Run();
        }
    }
}
