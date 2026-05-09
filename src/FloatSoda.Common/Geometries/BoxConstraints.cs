namespace FloatSoda.Common.Geometries;

using static Double;

public readonly record struct BoxConstraints(
    double MinWidth = 0,
    double MaxWidth = PositiveInfinity,
    double MinHeight = 0,
    double MaxHeight = PositiveInfinity)
{
    public static BoxConstraints Tight(Size size) => new(size.Width, size.Width, size.Height, size.Height);

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
}