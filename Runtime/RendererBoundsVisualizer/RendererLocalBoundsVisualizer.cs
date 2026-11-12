using UnityEngine;
using Display = RendererBoundsVisualizer.DisplayRendererBounds;
using static RendererBoundsVisualizer.RendererBoundsVisualizerManager;
using static RendererBoundsVisualizer.DisplayRendererBounds;

namespace RendererBoundsVisualizer {
    [ExecuteAlways]
    [AddComponentMenu("Renderer Bounds Visualizer/Local Bounds Visualizer")]
    internal class RendererLocalBoundsVisualizer : MonoBehaviour {
        public const string OverrideColorPropertyName = nameof(_overrideColor);
        public const string ColorPropertyName = nameof(_color);
        public const string OverrideDisplayPropertyName = nameof(_overrideDisplay);
        public const string DisplayPropertyName = nameof(_display);

        [SerializeField] bool _overrideColor;
        [SerializeField] Color _color = Color.gray4; // Same color as for SkinnedMeshRenderer local bounds
        [SerializeField] bool _overrideDisplay;
        [SerializeField] Display _display = OnlyWhenSelected;

#if UNITY_EDITOR
        Renderer _renderer;

        Color Color => _overrideColor ? _color : GlobalLocalBoundsColor ?? _color;
        Display Display => _overrideDisplay ? _display : GlobalLocalBoundsDisplay ?? _display;

        void Awake() => _renderer = GetComponent<Renderer>();

        void OnDrawGizmos() {
            if (!enabled)
                return;

            if (Display != Always)
                return;

            DrawLocalBounds();
        }

        void OnDrawGizmosSelected() {
            if (!enabled)
                return;

            if (Display != OnlyWhenSelected)
                return;

            DrawLocalBounds();
        }

        void DrawLocalBounds() {
            if (_renderer == null)
                _renderer = GetComponent<Renderer>();

            if (_renderer == null)
                return;

            var targetTransform = GetTargetTransform();
            if (targetTransform == null)
                return;

            var gizmosMatrix = Gizmos.matrix;

            try {
                Gizmos.color = Color;
                Gizmos.matrix = targetTransform.localToWorldMatrix;
                var localBounds = _renderer.localBounds;
                Gizmos.DrawWireCube(localBounds.center, localBounds.size);
            }
            finally {
                Gizmos.matrix = gizmosMatrix;
            }
        }

        Transform GetTargetTransform() {
            var skinnedMeshRenderer = _renderer as SkinnedMeshRenderer;
            if (skinnedMeshRenderer == null)
                return _renderer.transform;

            if (skinnedMeshRenderer.rootBone == null)
                return _renderer.transform;

            return skinnedMeshRenderer.rootBone;
        }
#endif
    }
}