using SkiaSharp;

namespace FloatSoda.Common.Geometries;

using static Double;
using static MathF;

public readonly record struct BoxConstraints(
    double MinWidth = 0,
    double MaxWidth = PositiveInfinity,
    double MinHeight = 0,
    double MaxHeight = PositiveInfinity)
{
    public static BoxConstraints Tight(SKSize size) => new(size.Width, size.Width, size.Height, size.Height);

    public static BoxConstraints TightFor(double? width, double? height) => new(
        width ?? 0,
        width ?? PositiveInfinity,
        height ?? 0,
        height ?? PositiveInfinity
    );

    public BoxConstraints Enforce(BoxConstraints constraints) => new(
        Math.Clamp(MinWidth, constraints.MinWidth, constraints.MaxWidth),
        Math.Clamp(MaxWidth, constraints.MinWidth, constraints.MaxWidth),
        Math.Clamp(MinHeight, constraints.MaxHeight, constraints.MaxHeight),
        Math.Clamp(MaxHeight, constraints.MaxHeight, constraints.MaxHeight)
    );

    public double ConstrainWidth(float width) => Math.Clamp(width, MinWidth, MaxWidth);
    public double ConstrainHeight(double height) => Math.Clamp(height, MinHeight, MaxHeight);

    public BoxConstraints Loosen => new(MaxWidth, MaxWidth, MaxHeight, MaxHeight);
    public SKSize Smallest => new((float)ConstrainWidth(0), (float)ConstrainHeight(0));

    public SKSize Constrain(SKSize size)
    {
        return new SKSize((float)ConstrainWidth(size.Width), (float)ConstrainHeight(size.Height));
    }
}