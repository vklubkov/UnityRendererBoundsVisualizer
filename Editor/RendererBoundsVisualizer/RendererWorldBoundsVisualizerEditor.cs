using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Display = RendererBoundsVisualizer.DisplayRendererBounds;
using static RendererBoundsVisualizer.RendererBoundsVisualizerManager;
using static RendererBoundsVisualizer.RendererWorldBoundsVisualizer;
using static UnityEngine.UIElements.DisplayStyle;

namespace RendererBoundsVisualizer {
    [CanEditMultipleObjects]
    [CustomEditor(typeof(RendererWorldBoundsVisualizer))]
    internal class RendererWorldBoundsVisualizerEditor : Editor {
        const string _scriptPropertyName = "m_Script";
        const string _globalDisplayText = "Global Display";
        const string _globalColorText = "Global Color";

        VisualElement _root;

        PropertyField _overrideDisplayField;
        PropertyField _displayField;
        EnumField _globalDisplayField;

        PropertyField _overrideColorField;
        PropertyField _colorField;
        ColorField _globalColorField;

        bool _hasGlobalDisplay;
        bool _hasGlobalColor;
        bool _isOverrideDisplay;
        bool _isOverrideColor;

        Action<bool> _onOverrideDisplayChanged;
        Action<bool> _onOverrideColorChanged;

        event Action<bool> OnOverrideDisplayChanged {
            add {
                _onOverrideDisplayChanged += value;
                value?.Invoke(_isOverrideDisplay);
            }
            remove => _onOverrideDisplayChanged -= value;
        }

        event Action<bool> OnOverrideColorChanged {
            add {
                _onOverrideColorChanged += value;
                value?.Invoke(_isOverrideColor);
            }
            remove => _onOverrideColorChanged -= value;
        }

        DisplayStyle OverrideDisplayFieldDisplayStyle => _hasGlobalDisplay ? Flex : None;
        DisplayStyle DisplayFieldDisplayStyle =>  _isOverrideDisplay || !_hasGlobalDisplay ? Flex : None;
        DisplayStyle GlobalDisplayFieldDisplayStyle => _hasGlobalDisplay && !_isOverrideDisplay ? Flex : None;

        DisplayStyle OverrideColorFieldDisplayStyle => _hasGlobalColor ? Flex : None;
        DisplayStyle ColorFieldDisplayStyle =>  _isOverrideColor || !_hasGlobalColor ? Flex : None;
        DisplayStyle GlobalColorFieldDisplayStyle => _hasGlobalColor && !_isOverrideColor ? Flex : None;

        public override VisualElement CreateInspectorGUI() {
            _hasGlobalDisplay = GlobalWorldBoundsDisplay.HasValue;
            _hasGlobalColor = GlobalWorldBoundsColor.HasValue;

            _root = new VisualElement();

            var overrideDisplayProperty = serializedObject.FindProperty(OverrideDisplayPropertyName);
            var overrideColorProperty = serializedObject.FindProperty(OverrideColorPropertyName);

            _isOverrideDisplay =
                _root.GetAndTrackPropertyValue(
                    overrideDisplayProperty,
                    GetOverrideDisplayValue,
                    OnOverrideDisplayProperty);

            _isOverrideColor =
                _root.GetAndTrackPropertyValue(
                    overrideColorProperty,
                    GetOverrideColorValue,
                    OnOverrideColorProperty);

            CreateScriptField();
            CreateVerticalSpacer();
            CreateOverrideDisplayField(overrideDisplayProperty);
            CreateDisplayField();
            CreateGlobalDisplayField();
            CreateVerticalSpacer();
            CreateOverrideColorField(overrideColorProperty);
            CreateColorField();
            CreateGlobalColorField();

            _root.RegisterCallback<AttachToPanelEvent>(OnAttach);
            _root.RegisterCallback<DetachFromPanelEvent>(OnDetach);

            _root.Bind(serializedObject);

            return _root;
        }

        bool GetOverrideDisplayValue(SerializedProperty overrideDisplayProperty) => overrideDisplayProperty.boolValue;

        void OnOverrideDisplayProperty(SerializedProperty overrideDisplayProperty) =>
            _onOverrideDisplayChanged?.Invoke(overrideDisplayProperty.boolValue);

        bool GetOverrideColorValue(SerializedProperty overrideColorProperty) => overrideColorProperty.boolValue;

        void OnOverrideColorProperty(SerializedProperty overrideColorProperty) =>
            _onOverrideColorChanged?.Invoke(overrideColorProperty.boolValue);

        void CreateScriptField() {
            var scriptProperty = serializedObject.FindProperty(_scriptPropertyName);
            var scriptPropertyField = new PropertyField(scriptProperty);
            scriptPropertyField.SetEnabled(false);
            _root.Add(scriptPropertyField);
        }

        void CreateOverrideDisplayField(SerializedProperty overrideDisplayProperty) {
            _overrideDisplayField = new PropertyField(overrideDisplayProperty);
            _overrideDisplayField.style.display = OverrideDisplayFieldDisplayStyle;
            _root.Add(_overrideDisplayField);
        }

        void CreateDisplayField() {
            var displayProperty = serializedObject.FindProperty(DisplayPropertyName);
            _displayField = new PropertyField(displayProperty);
            _displayField.style.display = DisplayFieldDisplayStyle;
            _root.Add(_displayField);
        }

        void CreateGlobalDisplayField() {
            _globalDisplayField = new EnumField(_globalDisplayText, Display.OnlyWhenSelected);
            _globalDisplayField.AddToClassList(EnumField.alignedFieldUssClassName);
            _globalDisplayField.SetEnabled(false);
            _globalDisplayField.style.display = GlobalDisplayFieldDisplayStyle;
            _root.Add(_globalDisplayField);
        }

        void CreateOverrideColorField(SerializedProperty overrideColorProperty) {
            _overrideColorField = new PropertyField(overrideColorProperty);
            _overrideColorField.style.display = OverrideColorFieldDisplayStyle;
            _root.Add(_overrideColorField);
        }

        void CreateColorField() {
            var colorProperty = serializedObject.FindProperty(ColorPropertyName);
            _colorField = new PropertyField(colorProperty);
            _colorField.style.display = ColorFieldDisplayStyle;
            _root.Add(_colorField);
        }

        void CreateGlobalColorField() {
            _globalColorField = new ColorField(_globalColorText);
            _globalColorField.AddToClassList(EnumField.alignedFieldUssClassName);
            _globalColorField.SetEnabled(false);
            _globalColorField.style.display = GlobalColorFieldDisplayStyle;
            _root.Add(_globalColorField);
        }

        void CreateVerticalSpacer() {
            var child = new VisualElement();
            child.style.height = 3;
            _root.Add(child);
        }

        void OnAttach(AttachToPanelEvent evt) {
            OnOverrideDisplayChanged += OnOverrideDisplayChangedHandler;
            OnOverrideColorChanged += OnOverrideColorChangedHandler;

            GlobalWorldBoundsDisplayChanged += OnGlobalWorldBoundsDisplayChangedHandler;
            GlobalWorldBoundsColorChanged += OnGlobalWorldBoundsColorChangedHandler;
        }

        void OnDetach(DetachFromPanelEvent evt) {
            OnOverrideDisplayChanged -= OnOverrideDisplayChangedHandler;
            OnOverrideColorChanged -= OnOverrideColorChangedHandler;

            GlobalWorldBoundsDisplayChanged -= OnGlobalWorldBoundsDisplayChangedHandler;
            GlobalWorldBoundsColorChanged -= OnGlobalWorldBoundsColorChangedHandler;
        }

        void OnOverrideDisplayChangedHandler(bool overrideDisplay) {
            _isOverrideDisplay = overrideDisplay;

            _overrideDisplayField.style.display = OverrideDisplayFieldDisplayStyle;
            _displayField.style.display = DisplayFieldDisplayStyle;
            _globalDisplayField.style.display = GlobalDisplayFieldDisplayStyle;
        }

        void OnOverrideColorChangedHandler(bool overrideColor) {
            _isOverrideColor = overrideColor;

            _overrideColorField.style.display = OverrideColorFieldDisplayStyle;
            _colorField.style.display = ColorFieldDisplayStyle;
            _globalColorField.style.display = GlobalColorFieldDisplayStyle;
        }

        void OnGlobalWorldBoundsDisplayChangedHandler(Display? display) {
            _hasGlobalDisplay = display.HasValue;

            _overrideDisplayField.style.display = OverrideDisplayFieldDisplayStyle;
            _displayField.style.display = DisplayFieldDisplayStyle;
            _globalDisplayField.style.display = GlobalDisplayFieldDisplayStyle;

            if (display.HasValue)
                _globalDisplayField.value = display.Value;
        }

        void OnGlobalWorldBoundsColorChangedHandler(Color? color) {
            _hasGlobalColor = color.HasValue;

            _overrideColorField.style.display = OverrideColorFieldDisplayStyle;
            _colorField.style.display = ColorFieldDisplayStyle;
            _globalColorField.style.display = GlobalColorFieldDisplayStyle;

            if (color.HasValue)
                _globalColorField.value = color.Value;
        }
    }
}