using SkiaSharp;

namespace FloatSoda.Engine.Painting;

public readonly record struct Size(float Width, float Height)
{
    public static readonly Size Zero = new(0, 0);
    public static readonly Size One = new(1, 1);

    public static Size FromRect(Rect rect) => new(
        Math.Abs(rect.Right - rect.Left),
        Math.Abs(rect.Bottom - rect.Top)
    );

    public static Size operator *(Size size, float x) => new Size(size.Width * x, size.Height * x);
    public static Size operator /(Size size, float x) => new(size.Width / x, size.Height / x);
}

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

public interface IRect
{
    float Left { get; }
    float Top { get; }
    float Right { get; }
    float Bottom { get; }

    Size Size { get; }
}

public readonly record struct Rect(float Left, float Top, float Right, float Bottom) : IRect
{
    public static readonly Rect Zero = new(0, 0, 0, 0);
    public static readonly Rect One = new(0, 0, 1, 1);

    public Size Size => Size.FromRect(this);

    public Offset TopLeft => new(Left, Top);
    public Offset Center => new(Left + Size.Width / 2f, Top + Size.Height / 2f);
    public Offset TopRight => new(Right, Top);
    public Offset BottomLeft => new(Left, Bottom);
    public Offset BottomRight => new(Right, Bottom);

    public static Rect LTRB(float left, float top, float right, float bottom) => new(left, top, right, bottom);

    public static Rect LTWH(float left, float top, float width, float height) => new(left, top, left + width, top + height);

    public static Rect FromCenter(Offset center, Size size) => LTWH(center.X - size.Width / 2f, center.Y - size.Height / 2f, size.Width, size.Height);

    public static Rect FromSizeAndOffset(Size size, Offset leftTop) => new(
        leftTop.X,
        leftTop.Y,
        leftTop.X + size.Width,
        leftTop.Y + size.Height
    );

    public static Rect Normalize(Rect rect)
    {
        var (left, right) = rect.Left > rect.Right ? (rect.Right, rect.Left) : (rect.Left, rect.Right);
        var (top, bottom) = rect.Top > rect.Bottom ? (rect.Bottom, rect.Top) : (rect.Top, rect.Bottom);
        return new Rect(left, top, right, bottom);
    }


    public Rect Resize(Size size) => FromSizeAndOffset(size, new Offset(Left, Top));

    public Rect Inflate(float delta) => LTRB(Left - delta, Top - delta, Right + delta, Bottom + delta);
    public Rect Deflate(float delta) => LTRB(Left + delta, Top + delta, Right - delta, Bottom - delta);
    public Rect Shift(Offset offset) => this + offset;
    public Rect Shift(float dx = 0, float dy = 0) => this + new Offset(dx, dy);

    public bool Contains(Offset p) => p.X >= Left && p.X <= Right && p.Y >= Top && p.Y <= Bottom;

    public static Rect operator +(Rect rect, Offset offset) => FromSizeAndOffset(rect.Size, rect.TopLeft + offset);
    public static Rect operator -(Rect rect, Offset offset) => FromSizeAndOffset(rect.Size, rect.TopLeft - offset);

    public static implicit operator SKRect(Rect rect) => new(rect.Left, rect.Top, rect.Right, rect.Bottom);
    public static implicit operator Rect(SKRect rect) => new(rect.Left, rect.Top, rect.Right, rect.Bottom);
}

public readonly record struct RRect(Rect Rect, float Rx, float Ry) : IRect
{
    public static readonly RRect Zero = new(Rect.Zero, 0, 0);

    public static RRect FromRectAndRadius(Rect rect, float radius) => new(rect, radius, radius);

    public RRect Shift(Offset offset) => this with { Rect = Rect.Shift(offset) };
    public RRect Inflate(float delta) => this with { Rect = Rect.Inflate(delta) };
    public RRect Deflate(float delta) => this with { Rect = Rect.Deflate(delta) };
    
    public float Left => Rect.Left;
    public float Top => Rect.Top;
    public float Right => Rect.Right;
    public float Bottom => Rect.Bottom;
    public Size Size => Rect.Size;

    public static implicit operator Rect(RRect rect) => rect.Rect;
    public static implicit operator RRect(Rect rect) => new(rect, 0, 0);
}