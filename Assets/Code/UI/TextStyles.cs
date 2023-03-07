using UnityEngine;
using TMPro;

namespace Icarus.UI {
    public enum TextStyle {
        Display1,
        Label1,
        Label1R,
        Label1L,
        KeyPad1,
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
            // Label1
            new TextStyleConfig {
                Bounds = new Vector2(0.45f, 0.06f),
                Size = 0.4f,
                Style = FontStyles.Normal,
                Color = new Color(0f, 0f, 0f),
                HAlign = HorizontalAlignmentOptions.Center,
                VAlign = VerticalAlignmentOptions.Middle,
            },
            // Label1R
            new TextStyleConfig {
                Bounds = new Vector2(0.45f, 0.06f),
                Size = 0.4f,
                Style = FontStyles.Normal,
                Color = new Color(0f, 0f, 0f),
                HAlign = HorizontalAlignmentOptions.Right,
                VAlign = VerticalAlignmentOptions.Middle,
            },
            // Label1L
            new TextStyleConfig {
                Bounds = new Vector2(0.45f, 0.06f),
                Size = 0.4f,
                Style = FontStyles.Normal,
                Color = new Color(0f, 0f, 0f),
                HAlign = HorizontalAlignmentOptions.Left,
                VAlign = VerticalAlignmentOptions.Middle,
            },
            // KeyPad1
            new TextStyleConfig {
                Bounds = new Vector2(0.45f, 0.06f),
                Size = 0.3f,
                Style = FontStyles.Normal,
                Color = new Color(1f, 1f, 1f),
                HAlign = HorizontalAlignmentOptions.Center,
                VAlign = VerticalAlignmentOptions.Middle,
            },
        };
    }
}
