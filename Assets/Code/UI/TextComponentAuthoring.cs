using UnityEngine;
using Unity.Entities;
using TMPro;

namespace Icarus.UI {
    public class TextComponentAuthoring : MonoBehaviour {
        public string key;
        public DatumType datumType;
        public string format = "{0}";
        public bool dynamic = true;
        public TextStyle style;
        public TMP_FontAsset font;
        
        public class TextComponentAuthoringBaker : Baker<TextComponentAuthoring> {
            public override void Bake(TextComponentAuthoring auth) {
                AddComponent<UninitializedDatumRef>(new UninitializedDatumRef {
                        ID = auth.key,
                        Type = auth.datumType,
                    });
                AddComponentObject<ManagedTextComponent>(new ManagedTextComponent {
                        GO = null,
                        Font = auth.font,
                        Style = auth.style,
                        Format = auth.format,
                    });
            }
        }
    }
}
