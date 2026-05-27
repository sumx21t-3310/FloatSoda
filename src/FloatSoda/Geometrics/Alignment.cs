using FloatSoda.Common.Geometries;
using SkiaSharp;

namespace FloatSoda.Geometrics;

public readonly record struct Alignment(float X = 0, float Y = 0)
{
    public static readonly Alignment TopLeft = new(-1, -1);
    public static readonly Alignment TopCenter = new(0, -1);
    public static readonly Alignment TopRight = new(1, -1);
    public static readonly Alignment CenterLeft = new(-1, 0);
    public static readonly Alignment Center = default;
    public static readonly Alignment CenterRight = new(1, 0);
    public static readonly Alignment BottomLeft = new(-1, 1);
    public static readonly Alignment BottomCenter = new(0, 1);
    public static readonly Alignment BottomRight = new(1, 1);


    public Offset ComputeOffset(SKSize parent, SKSize child)
    {
        var offset = (parent - child).ToPoint();
        return new Offset(
            (1 + X) * offset.X / 2,
            (1 + Y) * offset.Y / 2
        );
    }

    public Alignment FlipAll() => new(-X, -Y);
    public Alignment FlipX() => this with { X = -X };
    public Alignment FlipY() => this with { Y = -Y };
}

