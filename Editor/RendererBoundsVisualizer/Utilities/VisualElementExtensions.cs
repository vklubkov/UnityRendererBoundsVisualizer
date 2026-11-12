using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RendererBoundsVisualizer {
    public static class VisualElementExtensions {
        public static TValue GetAndTrackPropertyValue<TValue>(
            this VisualElement element,
            SerializedProperty serializedProperty,
            Func<SerializedProperty, TValue> getPropertyValue,
            Action<SerializedProperty> onTrackPropertyValue) {
            var value = getPropertyValue(serializedProperty);
            element.TrackPropertyValue(serializedProperty, onTrackPropertyValue);
            return value;
        }
    }
}