using net.puk06.CanvasAnimation.Models;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

namespace net.puk06.CanvasAnimation.Utils
{
    public class ColorUtils : UdonSharpBehaviour
    {
        public static Color GetColor(Component component, ElementType elementType)
        {
            switch (elementType)
            {
                case ElementType.Text:
                    Text text = (Text)component;
                    return text.color;
                case ElementType.TMP_Text:
                    TMP_Text tmp = (TMP_Text)component;
                    return tmp.color;
                case ElementType.Image:
                    Image img = (Image)component;
                    return img.color;
                case ElementType.RawImage:
                    RawImage raw = (RawImage)component;
                    return raw.color;
                case ElementType.Button:
                    Button btn = (Button)component;
                    return btn.targetGraphic.color;
                default:
                    return Color.white;
            }
        }

        public static void SetColor(Component component, ElementType elementType, Color color)
        {
            switch (elementType)
            {
                case ElementType.Text:
                    Text text = (Text)component;
                    text.color = color;
                    return;
                case ElementType.TMP_Text:
                    TMP_Text tmp = (TMP_Text)component;
                    tmp.color = color;
                    return;
                case ElementType.Image:
                    Image img = (Image)component;
                    img.color = color;
                    return;
                case ElementType.RawImage:
                    RawImage raw = (RawImage)component;
                    raw.color = color;
                    return;
                case ElementType.Button:
                    Button btn = (Button)component;
                    btn.targetGraphic.color = color;
                    return;
                default:
                    return;
            }
        }

        public static bool IsInvalidColor(Color color)
        {
            return float.IsPositiveInfinity(color.r)
                && float.IsPositiveInfinity(color.g)
                && float.IsPositiveInfinity(color.b)
                && float.IsPositiveInfinity(color.a);
        }

        public static Color GetInvalidColor()
        {
            return new Color(
                float.PositiveInfinity,
                float.PositiveInfinity,
                float.PositiveInfinity,
                float.PositiveInfinity
            );
        }

        public static Color ClampColor(float r, float g, float b, float a)
        {
            return new Color(
                r == float.PositiveInfinity ? r : r / 255f,
                g == float.PositiveInfinity ? g : g / 255f,
                b == float.PositiveInfinity ? b : b / 255f,
                a == float.PositiveInfinity ? a : a / 255f
            );
        }
    }
}
