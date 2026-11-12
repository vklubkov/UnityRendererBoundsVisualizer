using JetBrains.Annotations;

namespace RendererBoundsVisualizer {
    internal enum DisplayRendererBounds {
        [UsedImplicitly] Never,
        OnlyWhenSelected,
        Always
    }
}