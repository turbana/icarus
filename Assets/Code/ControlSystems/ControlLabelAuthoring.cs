using UnityEngine;
using Unity.Entities;
using TMPro;

namespace Icarus.UI {
    public class ControlLabelAuthoring : MonoBehaviour {
        public TextStyle Style;
        public TMP_FontAsset Font;
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
                AddComponentObject<ManagedTextComponent>(new ManagedTextComponent {
                        GO = null,
                        Font = auth.Font,
                        Style = auth.Style,
                        Format = auth.DatumID,
                    });
            }
        }
    }
}
