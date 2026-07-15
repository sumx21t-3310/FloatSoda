using SkiaSharp;

namespace FloatSoda.Rendering.Layers;

public interface ILayer
{
    public SKRect PaintBounds { get; }

    void Layout(LayerContext context);
    void Paint(LayerContext context);
    ILayer Clone();
}