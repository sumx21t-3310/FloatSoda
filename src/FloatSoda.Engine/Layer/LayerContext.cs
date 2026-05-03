using SkiaSharp;

namespace FloatSoda.Engine.Layer;

public readonly record struct LayerContext(SKCanvas Canvas)
{
    public static LayerContext Create(SKSurface surface) => new(surface.Canvas);
}