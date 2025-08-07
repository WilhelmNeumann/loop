using UnityEngine;

namespace UI
{
    public static class ColorPicker
    {
        private static readonly string[] HexStrings = new[]
        {
            "#14C8FF",
            "#9E0138",
            "#F46D43",
            "#FFFFBF",
            "#66C2A5",
            "#1C032B",
            "#3278FF",
            "#AAF050",
            "#C85AFF",
            "#FFB400"
        };
        
        private static readonly Color[] AllColors = {
            new(28,3,43),
            new(158,1,56),
            new(244,109,67),
            new(255,255,191),
            new(102,194,165),
        };

        public static Color GetColor(int i)
        {
            ColorUtility.TryParseHtmlString(HexStrings[i], out var color);
            return color;
        }
    }
}