using SkiaSharp;

namespace FloatSoda.Abstractions.Geometries;

public readonly record struct Offset(double X = 0, double Y = 0)
{
    public static Offset Zero => default;
    public static Offset One => new(1, 1);

    public static Offset operator +(Offset a, Offset b) => new(a.X + b.X, a.Y + b.Y);

    public static Offset operator -(Offset a, Offset b) => new(a.X - b.X, a.Y - b.Y);
    public static Offset operator -(Offset a) => new(-a.X, -a.Y);

    public static Offset operator *(Offset a, double b) => new(a.X * b, a.Y * b);
    public static Offset operator *(double a, Offset b) => b * a;

    public static Offset operator /(Offset a, double b) => new(a.X / b, a.Y / b);

    public static implicit operator Offset(SKPoint point) => new(point.X, point.Y);
    
    public static implicit operator SKPoint(Offset offset) => new ((float)offset.X, (float)offset.Y);
}