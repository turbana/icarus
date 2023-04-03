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

        public TextStyleData Data {
            get { return new TextStyleData(this); }
        }
    }

    public class TextStyleData {
        public Vector2 Bounds = new Vector2(0.1f, 0.1f);
        public float FontSize = 0.30f;
        public FontStyles FontStyle;
        public Color FontColor;
        public HorizontalAlignmentOptions HAlign;
        public VerticalAlignmentOptions VAlign;
        public TMP_FontAsset FontAsset;
        public Material FontMaterial;

        public TextStyleData() {}

        public TextStyleData(TextStyle other) {
            Bounds = other.Bounds;
            FontSize = other.FontSize;
            FontStyle = other.FontStyle;
            FontColor = other.FontColor;
            HAlign = other.HAlign;
            VAlign = other.VAlign;
            FontAsset = other.FontAsset;
            FontMaterial = other.FontMaterial;
        }
    }
}
