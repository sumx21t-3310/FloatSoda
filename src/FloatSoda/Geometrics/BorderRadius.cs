using SkiaSharp;

namespace FloatSoda.Geometrics;

public record struct BorderRadius(
    Radius TopLeft = default,
    Radius TopRight = default,
    Radius BottomRight = default,
    Radius BottomLeft = default)
{
    public static BorderRadius All(Radius radius) => new(radius, radius, radius, radius);
    public static readonly BorderRadius Zero = All(Radius.Zero);

    public static BorderRadius Circular(float radius) => All(Radius.Circular(radius));


    public SKRoundRect ToRoundRect(SKRect rect)
    {
        var roundRect = new SKRoundRect(rect);

        roundRect.SetRectRadii(rect, [TopLeft, TopRight, BottomRight, BottomLeft]);

        return roundRect;
    }
}

public record struct Radius(float X = 0, float Y = 0)
{
    public static Radius Circular(float radius) => new(radius, radius);
    public static readonly Radius Zero = new(0, 0);
    public static implicit operator SKPoint(Radius radius) => new(radius.X, radius.Y);
}