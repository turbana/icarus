using UnityEditor;
using System;

using UnityEngine;
using Unity.Entities;
using TMPro;

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
                var config = TextStylesConfig.Singleton;
                var style = config.LookupStyle(auth.FontStyle);
                if (style is null) {
                    Debug.LogWarning($"Could not find TextStyle: {auth.FontStyle}", auth);
                    return;
                }
                AddComponentObject<ManagedTextComponent>(new ManagedTextComponent {
                        GO = null,
                        Style = style,
                        Format = auth.DatumID,
                    });
            }
        }
    }

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
}
