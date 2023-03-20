using UnityEditor;
using System;

using UnityEngine;
using Unity.Entities;
using Unity.Entities.Hybrid.Baking;
using TMPro;

namespace Icarus.UI {
    public class TextComponentAuthoring : MonoBehaviour {
        public string datumKey;
        public DatumType datumType;
        [TextArea(4, 100)]
        public string format = "{0}";
        public string style;
        
        public class TextComponentAuthoringBaker : Baker<TextComponentAuthoring> {
            public override void Bake(TextComponentAuthoring auth) {
                var config = TextStylesConfig.Singleton;
                var style = config.LookupStyle(auth.style);
                if (style is null) {
                    Debug.LogWarning($"Could not find TextStyle: {auth.style}", auth);
                    return;
                }
                DependsOn(config);
                DependsOn(style);
                if (auth.datumKey != "") {
                    AddComponent<UninitializedDatumRef>(new UninitializedDatumRef {
                            ID = auth.datumKey,
                            Type = auth.datumType,
                        });
                }
                AddComponentObject<ManagedTextComponent>(new ManagedTextComponent {
                        GO = null,
                        Style = style,
                        Format = auth.format,
                    });
            }
        }
    }

    [CustomEditor(typeof(TextComponentAuthoring))]
    [CanEditMultipleObjects]
    public class TextComponentAuthoringEditor : Editor {
        SerializedProperty datumKey, datumType, format, style;

        protected void OnEnable() {
            datumKey = serializedObject.FindProperty("datumKey");
            datumType = serializedObject.FindProperty("datumType");
            format = serializedObject.FindProperty("format");
            style = serializedObject.FindProperty("style");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();
            EditorGUILayout.PropertyField(datumKey);
            EditorGUILayout.PropertyField(datumType);
            EditorGUILayout.PropertyField(format);
            var styles = TextStylesConfig.Singleton.AllStyles;
            Array.Sort(styles);
            var index = Array.IndexOf(styles, style.stringValue);
            var selected = EditorGUILayout.Popup("Font Style", index, styles);
            style.stringValue = styles[selected];
            serializedObject.ApplyModifiedProperties();
        }
    }
}
