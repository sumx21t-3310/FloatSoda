using FloatSoda.Abstractions.Geometries;
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

    public Offset Pivot(SKSize size)
    {
        var centerX = size.Width / 2f;
        var centerY = size.Height / 2f;

        var offsetX = centerX * X;
        var offsetY = centerY * Y;

        return new Offset(offsetX, offsetY);
    }

    public Offset ComputeOffset(SKSize parent, SKSize child)
    {
        var remainingSpace = new SKSize(parent.Width - child.Width, parent.Height - child.Height);
        var centerSpace = new Offset(remainingSpace.Width / 2f, remainingSpace.Height / 2f);
        return centerSpace + Pivot(remainingSpace);
    }

    public Alignment FlipAll() => new(-X, -Y);
    public Alignment FlipX() => this with { X = -X };
    public Alignment FlipY() => this with { Y = -Y };
}