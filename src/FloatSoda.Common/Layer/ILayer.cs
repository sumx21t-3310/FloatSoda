using SkiaSharp;

namespace FloatSoda.Common.Layer;

public interface ILayer
{
    public SKRect PaintBounds { get; }

    void Layout(LayerContext context);
    void Paint(LayerContext context);
}