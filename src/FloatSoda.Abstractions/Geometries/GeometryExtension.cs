using SkiaSharp;

namespace FloatSoda.Abstractions.Geometries;

public static class GeometryExtension
{
    public static SKRoundRect MakeOffset(this SKRoundRect roundRect, float dx, float dy)
    {
        var rect = roundRect.Rect;
        rect.Offset(dx, dy);
        
        var copy = new SKRoundRect();
        copy.SetRectRadii(rect, roundRect.Radii);

        return copy;
    }

    public static SKRoundRect MakeOffset(this SKRoundRect roundRect, Offset offset) => roundRect.MakeOffset((float)offset.X, (float)offset.Y);
}