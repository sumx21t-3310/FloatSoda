using SkiaSharp;

namespace FloatSoda;

public record struct BorderRadius(Radius TopLeft, Radius TopRight, Radius BottomRight, Radius BottomLeft)
{
    public static BorderRadius All(Radius radius) => new(radius, radius, radius, radius);
    public static BorderRadius Zero => All(Radius.Zero);

    public static BorderRadius Circular(float radius) => new()
    {
        TopLeft = Radius.Circular(radius),
        TopRight = Radius.Circular(radius),
        BottomLeft = Radius.Circular(radius),
        BottomRight = Radius.Circular(radius)
    };

    public SKRoundRect ToRoundRect(SKRect rect)
    {
        var rrect = new SKRoundRect();

        rrect.SetRectRadii(rect, [TopLeft, TopRight, BottomRight, BottomLeft]);

        return rrect;
    }
}

public record struct Radius(float X, float Y)
{
    public static Radius Zero => new(0, 0);

    public static Radius Elliptical(float x, float y) => new(x, y);

    public static Radius Circular(float radius) => new(radius, radius);

    public static implicit operator SKPoint(Radius radius) => new(radius.X, radius.Y);
}