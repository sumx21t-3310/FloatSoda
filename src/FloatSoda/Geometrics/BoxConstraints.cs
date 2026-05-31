using SkiaSharp;

namespace FloatSoda.Geometrics;

using static Double;

public readonly record struct BoxConstraints(
    double MinWidth = 0,
    double MaxWidth = PositiveInfinity,
    double MinHeight = 0,
    double MaxHeight = PositiveInfinity)
{
    public static BoxConstraints Tight(SKSize size) => new(size.Width, size.Width, size.Height, size.Height);
    
    public static BoxConstraints Tight(double width, double height) => new(width, width, height, height);

    public static BoxConstraints TightFor(double? width = null, double? height = null) => new(
        width ?? 0,
        width ?? PositiveInfinity,
        height ?? 0,
        height ?? PositiveInfinity
    );

    public BoxConstraints Enforce(BoxConstraints constraints) => new(
        Math.Clamp(MinWidth, constraints.MinWidth, constraints.MaxWidth),
        Math.Clamp(MaxWidth, constraints.MinWidth, constraints.MaxWidth),
        Math.Clamp(MinHeight, constraints.MinHeight, constraints.MaxHeight), // ← 修正
        Math.Clamp(MaxHeight, constraints.MinHeight, constraints.MaxHeight) // ← 修正
    );


    public double ConstrainWidth(float width) => Math.Clamp(width, MinWidth, MaxWidth);
    public double ConstrainHeight(double height) => Math.Clamp(height, MinHeight, MaxHeight);

    public BoxConstraints Loosen => new(0, MaxWidth, 0, MaxHeight);
    public SKSize Smallest => new((float)ConstrainWidth(0), (float)ConstrainHeight(0));

    public SKSize Constrain(SKSize size)
    {
        return new SKSize((float)ConstrainWidth(size.Width), (float)ConstrainHeight(size.Height));
    }
}