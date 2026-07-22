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

    public static SKRoundRect MakeOffset(this SKRoundRect roundRect, Offset offset) =>
        roundRect.MakeOffset((float)offset.X, (float)offset.Y);

    public static bool Contains(this SKSize size, Offset offset)
    {
        return offset.X >= 0 && offset.X < size.Width && offset.Y >= 0 && offset.Y < size.Height;
    }
}
