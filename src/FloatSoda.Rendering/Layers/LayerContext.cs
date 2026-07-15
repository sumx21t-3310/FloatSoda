using SkiaSharp;

namespace FloatSoda.Rendering.Layers;

public readonly record struct LayerContext(SKCanvas Canvas)
{
    public static LayerContext Create(SKSurface surface) => new(surface.Canvas);
}