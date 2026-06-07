using SkiaSharp;

namespace FloatSoda.Geometrics;

using static Double;

public readonly record struct BoxConstraints(
    double MinWidth = 0,
    double MaxWidth = PositiveInfinity,
    double MinHeight = 0,
    double MaxHeight = PositiveInfinity)
{
    public static BoxConstraints Tight(double width, double height) => new(width, width, height, height);

    public static BoxConstraints Tight(SKSize size) => Tight(size.Width, size.Height);

    public static BoxConstraints TightFor(double? width = null, double? height = null) => new()
    {
        MinWidth = width ?? 0,
        MaxWidth = width ?? PositiveInfinity,
        MinHeight = height ?? 0,
        MaxHeight = height ?? PositiveInfinity
    };

    public BoxConstraints Enforce(BoxConstraints constraints) => new()
    {
        MinWidth = Clamp(MinWidth, constraints.MinWidth, constraints.MaxWidth),
        MaxWidth = Clamp(MaxWidth, constraints.MinWidth, constraints.MaxWidth),
        MinHeight = Clamp(MinHeight, constraints.MinHeight, constraints.MaxHeight),
        MaxHeight = Clamp(MaxHeight, constraints.MinHeight, constraints.MaxHeight)
    };

    public double ConstrainWidth(float width) => Clamp(width, MinWidth, MaxWidth);
    public double ConstrainHeight(double height) => Clamp(height, MinHeight, MaxHeight);

    public BoxConstraints Loosen => new(0, MaxWidth, 0, MaxHeight);
    public SKSize Smallest => new((float)ConstrainWidth(0), (float)ConstrainHeight(0));

    public SKSize Constrain(SKSize size) => Constrain(size.Width, size.Height);
    public SKSize Constrain(double width, double height) => new((float)width, (float)height);


    public override string ToString()
    {
        return
            $"BoxConstraints {{ width: (min = {MinWidth}, max = {MaxWidth}), height: (min = {MinHeight}, max = {MaxHeight}) }}";
    }
}