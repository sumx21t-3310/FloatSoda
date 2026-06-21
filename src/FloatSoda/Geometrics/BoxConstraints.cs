using SkiaSharp;

namespace FloatSoda.Geometrics;

using static Double;

public readonly record struct BoxConstraints(
    double MinWidth = 0,
    double MaxWidth = PositiveInfinity,
    double MinHeight = 0,
    double MaxHeight = PositiveInfinity)
{
    public static BoxConstraints Tight(SKSize size) => Tight(size.Width, size.Height);

    public static BoxConstraints Tight(double width, double height) => new(width, width, height, height);

    public static BoxConstraints TightFor(double? width = null, double? height = null) => new()
    {
        MinWidth = width ?? 0,
        MaxWidth = width ?? PositiveInfinity,
        MinHeight = height ?? 0,
        MaxHeight = height ?? PositiveInfinity
    };

    public static BoxConstraints Loose(double width, double height) => new()
    {
        MaxHeight = height,
        MaxWidth = width
    };

    public static BoxConstraints Loose(SKSize size) => Loose(size.Width, size.Height);


    public BoxConstraints Enforce(BoxConstraints constraints) => new(
        Math.Clamp(MinWidth, constraints.MinWidth, constraints.MaxWidth),
        Math.Clamp(MaxWidth, constraints.MinWidth, constraints.MaxWidth),
        Math.Clamp(MinHeight, constraints.MinHeight, constraints.MaxHeight),
        Math.Clamp(MaxHeight, constraints.MinHeight, constraints.MaxHeight)
    );

    public double ConstrainWidth(double width) => Math.Clamp(width, MinWidth, MaxWidth);
    public double ConstrainHeight(double height) => Math.Clamp(height, MinHeight, MaxHeight);

    public BoxConstraints Loosen => new(0, MaxWidth, 0, MaxHeight);
    public SKSize Smallest => new((float)ConstrainWidth(0), (float)ConstrainHeight(0));

    public SKSize Constrain(SKSize size) => Constrain(size.Width, size.Height);

    public SKSize Constrain(double width, double height) => new(
        (float)ConstrainWidth(width),
        (float)ConstrainHeight(height)
    );


    public bool HasTightWidth => MinWidth >= MaxWidth;
    public bool HasTightHeight => MinHeight >= MaxHeight;

    public bool IsTight => HasTightWidth && HasTightHeight;

    public override string ToString()
    {
        return $"BoxConstraints {{ width: (min = {MinWidth}, max = {MaxWidth}), height: (min = {MinHeight}, max = {MaxHeight}) }}";
    }
}