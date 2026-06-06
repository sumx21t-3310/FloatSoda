using FloatSoda.Common.Geometries;
using SkiaSharp;

namespace FloatSoda.Geometrics;

public static class RectExtension
{
    public static SKRect And(this SKSize rect, Offset other) =>
        SKRect.Create((float)other.X, (float)other.Y, rect.Width, rect.Height);
}