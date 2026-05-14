namespace FloatSoda.Common.Geometries;

public readonly record struct Offset(double X, double Y)
{
    public static Offset Zero => new(0, 0);
    public static Offset One => new(1, 1);

    public static Offset operator +(Offset a, Offset b) => new(a.X + b.X, a.Y + b.Y);

    public static Offset operator -(Offset a, Offset b) => new(a.X - b.X, a.Y - b.Y);

    public static Offset operator *(Offset a, double b) => new(a.X * b, a.Y * b);
    public static Offset operator *(double a, Offset b) => b * a;

    public static Offset operator /(Offset a, double b) => new(a.X / b, a.Y / b);
}