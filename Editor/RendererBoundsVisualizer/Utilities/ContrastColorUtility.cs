using UnityEngine;

namespace RendererBoundsVisualizer {
    // Constants here were proposed by AI, but they are okay.
    // I also tried luma (without linearizing, and checking
    // against 0.46 or 0.5 instead) but it seemed a bit off.
    public static class ContrastColorUtility {
        public static Color GetContrastingTextColor(this Color background) {
            var r = Linearize(background.r);
            var g = Linearize(background.g);
            var b = Linearize(background.b);
            var luminance = 0.2126f * r + 0.7152f * g + 0.0722f * b;
            return luminance > 0.179f ? Color.black : Color.white;
        }

        static float Linearize(float channel) =>
            channel <= 0.04045f
                ? channel / 12.92f
                : Mathf.Pow((channel + 0.055f) / 1.055f, 2.4f);
    }
}