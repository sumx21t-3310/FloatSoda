using SkiaSharp;

namespace FloatSoda;

public record struct BorderRadius(Radius TopLeft, Radius TopRight, Radius BottomRight, Radius BottomLeft)
{
    public static BorderRadius All(Radius radius) => new(radius, radius, radius, radius);
    public static BorderRadius Zero => All(Radius.Zero);

    public static BorderRadius Circular(float radius) => All(Radius.Circular(radius));

    public static BorderRadius Only(
        Radius topLeft = default,
        Radius topRight = default,
        Radius bottomRight = default,
        Radius bottomLeft = default)
    {
        return new(topLeft, topRight, bottomRight, bottomLeft);
    }

    public SKRoundRect ToRoundRect(SKRect rect)
    {
        var roundRect = new SKRoundRect(rect);

        roundRect.SetRectRadii(rect, [TopLeft, TopRight, BottomRight, BottomLeft]);

        return roundRect;
    }
}

public record struct Radius(float X, float Y)
{
    public static Radius Circular(float radius) => new(radius, radius);
    public static Radius Elliptical(float x, float y) => new(x, y);
    public static Radius Zero => new(0, 0);
    public static implicit operator SKPoint(Radius radius) => new(radius.X, radius.Y);
}