using UnityEngine;
using Unity.Entities;
using Unity.Entities.Hybrid.Baking;
using TMPro;

namespace Icarus.UI {
    public class TextComponentAuthoring : MonoBehaviour {
        public string datumKey;
        public DatumType datumType;
        public string format = "{0}";
        public TextStyle style;
        public TMP_FontAsset font;
        
        public class TextComponentAuthoringBaker : Baker<TextComponentAuthoring> {
            public override void Bake(TextComponentAuthoring auth) {
                if (auth.datumKey != "") {
                    AddComponent<UninitializedDatumRef>(new UninitializedDatumRef {
                            ID = auth.datumKey,
                            Type = auth.datumType,
                        });
                }
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
