namespace FloatSoda.Common.Geometries;

public readonly record struct Offset(float X, float Y)
{
    public static Offset Zero => new(0, 0);
    public static Offset One => new(1, 1);

    public static Offset operator +(Offset a, Offset b) => new(a.X + b.X, a.Y + b.Y);

    public static Offset operator -(Offset a, Offset b) => new(a.X - b.X, a.Y - b.Y);

    public static Offset operator *(Offset a, float b) => new(a.X * b, a.Y * b);
    public static Offset operator *(float a, Offset b) => b * a;

    public static Offset operator /(Offset a, float b) => new(a.X / b, a.Y / b);
}