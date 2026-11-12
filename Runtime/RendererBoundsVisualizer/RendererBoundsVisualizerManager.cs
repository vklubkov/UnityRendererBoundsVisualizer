using System;
using JetBrains.Annotations;
using Unity.Scripting.LifecycleManagement;
using UnityEditor;
using UnityEngine;
using Display = RendererBoundsVisualizer.DisplayRendererBounds;
using static UnityEngine.Debug;

namespace RendererBoundsVisualizer {
    [PublicAPI]
    [ExecuteAlways]
    [NoAutoStaticsCleanup]
    [AddComponentMenu("Renderer Bounds Visualizer/Bounds Visualizers Manager")]
    internal class RendererBoundsVisualizerManager : MonoBehaviour {
        public const string WorldBoundsDisplayPropertyName = nameof(_worldBoundsDisplay);
        public const string WorldBoundsColorPropertyName = nameof(_worldBoundsColor);
        public const string LocalBoundsDisplayPropertyName = nameof(_localBoundsDisplay);
        public const string LocalBoundsColorPropertyName = nameof(_localBoundsColor);

        [SerializeField] Display _worldBoundsDisplay = Display.OnlyWhenSelected;
        [SerializeField] Color _worldBoundsColor = Color.lightGoldenRod;
        [SerializeField] Display _localBoundsDisplay = Display.OnlyWhenSelected;
        [SerializeField] Color _localBoundsColor = Color.gray4; // Same color as for SkinnedMeshRenderer local bounds

#if UNITY_EDITOR

        static RendererBoundsVisualizerManager _instance;

        Display _oldWorldBoundsDisplay;
        Color _oldWorldBoundsColor;
        Display _oldLocalBoundsDisplay;
        Color _oldLocalBoundsColor;

        static Action<Display?> _worldBoundsDisplayChanged;
        static Action<Color?> _worldBoundsColorChanged;
        static Action<Display?> _localBoundsDisplayChanged;
        static Action<Color?> _localBoundsColorChanged;

        public static event Action<Display?> GlobalWorldBoundsDisplayChanged {
            add {
                _worldBoundsDisplayChanged += value;
                value?.Invoke(GlobalWorldBoundsDisplay);
            }
            remove => _worldBoundsDisplayChanged -= value;
        }

        public static event Action<Color?> GlobalWorldBoundsColorChanged {
            add {
                _worldBoundsColorChanged += value;
                value?.Invoke(GlobalWorldBoundsColor);
            }
            remove => _worldBoundsColorChanged -= value;
        }

        public static event Action<Display?> GlobalLocalBoundsDisplayChanged {
            add {
                _localBoundsDisplayChanged += value;
                value?.Invoke(GlobalLocalBoundsDisplay);
            }
            remove => _localBoundsDisplayChanged -= value;
        }

        public static event Action<Color?> GlobalLocalBoundsColorChanged {
            add {
                _localBoundsColorChanged += value;
                value?.Invoke(GlobalLocalBoundsColor);
            }
            remove => _localBoundsColorChanged -= value;
        }

        public static Display? GlobalWorldBoundsDisplay => _instance?._worldBoundsDisplay;
        public static Color? GlobalWorldBoundsColor => _instance?._worldBoundsColor;
        public static Display? GlobalLocalBoundsDisplay => _instance?._localBoundsDisplay;
        public static Color? GlobalLocalBoundsColor => _instance?._localBoundsColor;

        void OnEnable() {
            if (_instance != null && _instance != this) {
                LogWarning("Multiple RendererBoundsVisualizerManager instances can't co-exist in one scene!");

                EditorApplication.delayCall += () => {
                    if (Application.isPlaying)
                        Destroy(this);
                    else
                        DestroyImmediate(this);
                };

                return;
            }

            _instance = this;

            _worldBoundsDisplayChanged?.Invoke(GlobalWorldBoundsDisplay);
            _worldBoundsColorChanged?.Invoke(GlobalWorldBoundsColor);
            _localBoundsDisplayChanged?.Invoke(GlobalLocalBoundsDisplay);
            _localBoundsColorChanged?.Invoke(GlobalLocalBoundsColor);
        }

        void OnDisable() {
            if (_instance != this)
                return;

            _instance = null;

            _worldBoundsDisplayChanged?.Invoke(GlobalWorldBoundsDisplay);
            _worldBoundsColorChanged?.Invoke(GlobalWorldBoundsColor);
            _localBoundsDisplayChanged?.Invoke(GlobalLocalBoundsDisplay);
            _localBoundsColorChanged?.Invoke(GlobalLocalBoundsColor);
        }

        void Update() {
            if (_worldBoundsDisplay != _oldWorldBoundsDisplay) {
                _oldWorldBoundsDisplay = _worldBoundsDisplay;
                _worldBoundsDisplayChanged?.Invoke(GlobalWorldBoundsDisplay);
            }

            if (_worldBoundsColor != _oldWorldBoundsColor) {
                _oldWorldBoundsColor = _worldBoundsColor;
                _worldBoundsColorChanged?.Invoke(GlobalWorldBoundsColor);
            }

            if (_localBoundsDisplay != _oldLocalBoundsDisplay) {
                _oldLocalBoundsDisplay = _localBoundsDisplay;
                _localBoundsDisplayChanged?.Invoke(GlobalLocalBoundsDisplay);
            }

            if (_localBoundsColor != _oldLocalBoundsColor) {
                _oldLocalBoundsColor = _localBoundsColor;
                _localBoundsColorChanged?.Invoke(GlobalLocalBoundsColor);
            }
        }
#endif
    }
}