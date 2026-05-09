namespace FloatSoda.Common.Geometries;

public readonly record struct Size(float Width, float Height)
{
    public static readonly Size Zero = new(0, 0);
    public static readonly Size One = new(1, 1);

    public static Size FromRect(Rect rect) => new(MathF.Abs(rect.Right - rect.Left), MathF.Abs(rect.Bottom - rect.Top));

    public static Size operator *(Size size, float x) => new Size(size.Width * x, size.Height * x);
    public static Size operator /(Size size, float x) => new(size.Width / x, size.Height / x);
}