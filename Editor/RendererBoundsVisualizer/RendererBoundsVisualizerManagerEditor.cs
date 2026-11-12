using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static System.StringSplitOptions;
using static RendererBoundsVisualizer.RendererBoundsVisualizerManager;
using static UnityEngine.UIElements.VisualElement.MeasureMode;

namespace RendererBoundsVisualizer {
    [CustomEditor(typeof(RendererBoundsVisualizerManager))]
    internal class RendererBoundsVisualizerManagerEditor : Editor {
        const string _scriptPropertyName = "m_Script";
        const string _worldBoundsText = "World Bounds";
        const string _localBoundsText = "Local Bounds";
        const string _removeAllWorldBoundsButtonText = "Remove All World Bounds Visualizers";
        const string _removeAllLocalBoundsButtonText = "Remove All Local Bounds Visualizers";

        VisualElement _root;
        Button _removeAllWorldBoundsButton;
        Button _removeAllLocalBoundsButton;

        public override VisualElement CreateInspectorGUI() {
            _root = new VisualElement();
            CreateScriptField();

            CreateTitleLabel(_worldBoundsText);
            CreateOverrideField(WorldBoundsDisplayPropertyName);
            CreateOverrideField(WorldBoundsColorPropertyName);
            CreateRemoveAllWorldBoundsButton();

            CreateSpacer(_root);

            CreateTitleLabel(_localBoundsText);
            CreateOverrideField(LocalBoundsDisplayPropertyName);
            CreateOverrideField(LocalBoundsColorPropertyName);
            CreateRemoveAlLocalBoundsButton();

            return _root;
        }

        void CreateScriptField() {
            var scriptProperty = serializedObject.FindProperty(_scriptPropertyName);
            var scriptPropertyField = new PropertyField(scriptProperty);
            scriptPropertyField.SetEnabled(false);
            _root.Add(scriptPropertyField);
        }

        void CreateTitleLabel(string title) {
            var label = new Label(title);
            label.style.marginLeft = 3;
            label.style.paddingLeft = 1;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            _root.Add(label);
        }

        void CreateOverrideField(string propertyName) {
            var property = serializedObject.FindProperty(propertyName);
            var field = new PropertyField(property);
            _root.Add(field);
        }

        static void CreateSpacer(VisualElement parent) {
            var spacer = new VisualElement();
            spacer.style.height = 10;
            parent.Add(spacer);
        }

        void CreateRemoveAllWorldBoundsButton() {
            var worldBoundsColorProperty = serializedObject.FindProperty(WorldBoundsColorPropertyName);

            _removeAllWorldBoundsButton =
                CreateRemoveButton<RendererWorldBoundsVisualizer>(
                    _root, _removeAllWorldBoundsButtonText);

            var backgroundColor =
                _removeAllWorldBoundsButton.GetAndTrackPropertyValue(
                    worldBoundsColorProperty, GetColor, OnTrackWorldBoundsColorProperty);

            ApplyButtonColors(_removeAllWorldBoundsButton, backgroundColor);
        }

        void CreateRemoveAlLocalBoundsButton() {
            var localBoundsColorProperty = serializedObject.FindProperty(LocalBoundsColorPropertyName);

            _removeAllLocalBoundsButton =
                CreateRemoveButton<RendererLocalBoundsVisualizer>(
                    _root, _removeAllLocalBoundsButtonText);

            var backgroundColor =
                _removeAllLocalBoundsButton.GetAndTrackPropertyValue(
                    localBoundsColorProperty, GetColor, OnTrackLocalBoundsColorProperty);

            ApplyButtonColors(_removeAllLocalBoundsButton, backgroundColor);
        }

        static Color GetColor(SerializedProperty property) => property.colorValue;

        void OnTrackWorldBoundsColorProperty(SerializedProperty property) =>
            ApplyButtonColors(_removeAllWorldBoundsButton, property.colorValue);

        void OnTrackLocalBoundsColorProperty(SerializedProperty property) =>
            ApplyButtonColors(_removeAllLocalBoundsButton, property.colorValue);

        static void ApplyButtonColors(Button button, Color backgroundColor) {
            var color = backgroundColor.GetContrastingTextColor();
            button.style.backgroundColor = backgroundColor;
            button.style.color = color;
        }

        static Button CreateRemoveButton<T>(VisualElement parent, string text)
            where T : MonoBehaviour {
            var button = new Button(Remove<T>);
            button.text = text;
            button.style.unityFontStyleAndWeight = FontStyle.Bold;
            button.style.whiteSpace = WhiteSpace.Normal;
            button.style.flexGrow = 1;
            button.style.flexShrink = 1;
            button.style.height = StyleKeyword.Auto;
            button.style.marginLeft = 3;
            button.style.marginRight = -2;
            button.RegisterCallbackOnce<GeometryChangedEvent>(OnRemoveButtonGeometryChangedOnce);
            button.RegisterCallback<ChangeEvent<string>>(OnRemoveButtonLabelChanged);
            parent.Add(button);
            return button;
        }

        static void OnRemoveButtonGeometryChangedOnce(GeometryChangedEvent evt) {
            if (evt.currentTarget is not Button button)
                return;

            UpdateButtonMinWidth(button, evt.newRect, didLabelChange: true);
            button.RegisterCallback<GeometryChangedEvent>(OnRemoveButtonGeometryChanged);
        }

        static void OnRemoveButtonGeometryChanged(GeometryChangedEvent evt) {
            if (evt.currentTarget is not Button button)
                return;

            UpdateButtonMinWidth(button, evt.newRect, didLabelChange: false);
        }

        static void OnRemoveButtonLabelChanged(ChangeEvent<string> evt) {
            if (evt.currentTarget is not Button button)
                return;

            UpdateButtonMinWidth(button, button.layout, didLabelChange: true);
        }

        static void UpdateButtonMinWidth(Button button, Rect currentRect, bool didLabelChange) {
            var height = currentRect.height;
            if (string.IsNullOrEmpty(button.text)) { // Avoid collapsing empty buttons
                button.style.minWidth = height > 0 ? height : 0;
                return;
            }

            var splitButtonText = button.text.Split(new[] { ' ', '\t', '\n', '\r' }, RemoveEmptyEntries);
            var isSingleLine = splitButtonText.Length <= 1;

            float minWidth;
            if (!didLabelChange)
                minWidth = button.resolvedStyle.minWidth.value;
            else {
                var longestWord = splitButtonText
                    .OrderByDescending(text => text.Length)
                    .FirstOrDefault() ?? string.Empty;

                var textSize = button.MeasureTextSize(
                    textToMeasure: longestWord,
                    width: 0,
                    widthMode: Undefined,
                    height: 0,
                    heightMode: Undefined
                );

                var currentStyle = button.resolvedStyle;

                var additionalSpace =
                    currentStyle.paddingLeft + currentStyle.paddingRight +
                    currentStyle.borderLeftWidth + currentStyle.borderRightWidth
                    + 2f; // Additional padding for safety

                minWidth = Mathf.Ceil(textSize.x + additionalSpace);
            }

            if (isSingleLine && minWidth < height) // Make single-line buttons at least as wide as their height
                button.style.minWidth = height > 0 ? height : 0;
            else
                button.style.minWidth = minWidth > 0 ? minWidth : 0;
        }

        static void Remove<T>() where T : MonoBehaviour {
            var componentsToRemove = FindObjectsByType<T>(FindObjectsInactive.Include);
            foreach (var component in componentsToRemove) {
                if (Application.isPlaying)
                    Destroy(component);
                else
                    Undo.DestroyObjectImmediate(component);
            }
        }
    }
}