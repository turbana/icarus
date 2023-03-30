using UnityEngine;
using Unity.Entities;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
using System;
#endif

namespace Icarus.UI {
    public class ControlLabelAuthoring : MonoBehaviour {
        public string FontStyle;
        [Tooltip("Use the name of the Nth GameObject ancestor as the label text")]
        public int Ancestors = 0;

        public string DatumID {
            get {
                var go = this.gameObject;
                for (var i=Ancestors; i>0; i--) go = go.transform.parent.gameObject;
                return go.name;
            }
        }

        public class ControlLabelAuthoringBaker : Baker<ControlLabelAuthoring> {
            public override void Bake(ControlLabelAuthoring auth) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                var config = TextStylesConfig.Singleton;
                var style = config.LookupStyle(auth.FontStyle);
                if (style is null) {
                    Debug.LogWarning($"Could not find TextStyle: {auth.FontStyle}", auth);
                    return;
                }
                AddComponentObject<ManagedTextComponent>(entity, new ManagedTextComponent {
                        GO = null,
                        Style = style,
                        Format = auth.DatumID,
                    });
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ControlLabelAuthoring))]
    [CanEditMultipleObjects]
    public class ControlLabelAuthoringEditor : Editor {
        SerializedProperty FontStyle, Ancestors;

        protected void OnEnable() {
            FontStyle = serializedObject.FindProperty("FontStyle");
            Ancestors = serializedObject.FindProperty("Ancestors");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();
            EditorGUILayout.PropertyField(Ancestors);
            var styles = TextStylesConfig.Singleton.AllStyles;
            Array.Sort(styles);
            var index = Array.IndexOf(styles, FontStyle.stringValue);
            var selected = EditorGUILayout.Popup("Font Style", index, styles);
            FontStyle.stringValue = styles[selected];
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
