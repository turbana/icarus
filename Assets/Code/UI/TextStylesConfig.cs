using UnityEngine;
using UnityEditor;

namespace Icarus.UI {
    public class TextStylesConfig : MonoBehaviour {
        public static TextStylesConfig Singleton { get; private set; }
        private void OnValidate() { 
            if (Singleton != null && Singleton != this) { 
                Destroy(this); 
            } else { 
                Singleton = this; 
            } 
        }

        public static TextStyle LookupStyle(string StyleName) {
            if (StyleName is null || StyleName == "") { return null; }
            // if (Singleton is null) Debug.LogWarning("singleton is null)");
            // if (Singleton.transform is null) Debug.LogWarning("singleton transform is null");
            var child = Singleton.transform.Find(StyleName);
            if (child is null) { return null; }
            return child.GetComponent<TextStyle>();
        }

        public string[] AllStyles {
            get {
                var trans = Singleton.transform;
                string[] styles = new string[trans.childCount];
                for (int i=0; i<trans.childCount; i++) {
                    styles[i] = trans.GetChild(i).gameObject.name;
                }
                return styles;
            }
        }
    }
}
