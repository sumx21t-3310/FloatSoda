using FloatSoda.Common.Geometries;
using SkiaSharp;

namespace FloatSoda.Render;

public static class GeometryExtension
{
    public static SKRect And(this SKSize rect, Offset other) => new(
        (float)other.X,
        (float)other.Y,
        rect.Width,
        rect.Height
    );
}