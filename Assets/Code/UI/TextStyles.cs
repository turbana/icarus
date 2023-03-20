using UnityEngine;
using TMPro;

namespace Icarus.UI {
    public class TextStyle : MonoBehaviour {
        public Vector2 Bounds = new Vector2(0.1f, 0.1f);
        public float FontSize = 0.30f;
        public FontStyles FontStyle;
        public Color FontColor;
        public HorizontalAlignmentOptions HAlign;
        public VerticalAlignmentOptions VAlign;
        public TMP_FontAsset FontAsset;
        public Material FontMaterial;
    }
}
