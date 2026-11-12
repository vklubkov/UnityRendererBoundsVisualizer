using UnityEngine;
using Display = RendererBoundsVisualizer.DisplayRendererBounds;
using static RendererBoundsVisualizer.RendererBoundsVisualizerManager;
using static RendererBoundsVisualizer.DisplayRendererBounds;

namespace RendererBoundsVisualizer {
    [ExecuteAlways]
    [AddComponentMenu("Renderer Bounds Visualizer/World Bounds Visualizer")]
    internal class RendererWorldBoundsVisualizer : MonoBehaviour {
        public const string OverrideColorPropertyName = nameof(_overrideColor);
        public const string ColorPropertyName = nameof(_color);
        public const string OverrideDisplayPropertyName = nameof(_overrideDisplay);
        public const string DisplayPropertyName = nameof(_display);

        [SerializeField] bool _overrideColor;
        [SerializeField] Color _color = Color.lightGoldenRod;
        [SerializeField] bool _overrideDisplay;
        [SerializeField] Display _display = OnlyWhenSelected;

#if UNITY_EDITOR
        Renderer _renderer;

        Color Color => _overrideColor ? _color : GlobalWorldBoundsColor ?? _color;
        Display Display => _overrideDisplay ? _display : GlobalWorldBoundsDisplay ?? _display;

        void Awake() => _renderer = GetComponent<Renderer>();

        void OnDrawGizmos() {
            if (!enabled)
                return;

            if (Display != Always)
                return;

            DrawWorldBounds();
        }

        void OnDrawGizmosSelected() {
            if (!enabled)
                return;

            if (Display != OnlyWhenSelected)
                return;

            DrawWorldBounds();
        }

        void DrawWorldBounds() {
            if (_renderer == null)
                _renderer = GetComponent<Renderer>();

            if (_renderer == null)
                return;

            Gizmos.color = Color;
            var bounds = _renderer.bounds;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
#endif
    }
}