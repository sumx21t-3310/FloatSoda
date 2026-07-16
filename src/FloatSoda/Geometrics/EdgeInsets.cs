using FloatSoda.Abstractions.Geometries;

namespace FloatSoda.Geometrics;

public readonly record struct EdgeInsets(double Left = 0, double Top = 0, double Right = 0, double Bottom = 0)
{
    public static readonly EdgeInsets Zero = All(0);
    public static EdgeInsets All(double value) => new(value, value, value, value);

    public static EdgeInsets Symmetric(double vertical = 0, double horizontal = 0) => new()
    {
        Top = vertical,
        Bottom = vertical,
        Left = horizontal,
        Right = horizontal
    };

    public Offset TopLeft => new(Left, Top);
    public Offset TopRight => new(Right, Top);
    public Offset BottomLeft => new(Left, Bottom);
    public Offset BottomRight => new(Right, Bottom);
    
    public EdgeInsets Flipped => new(Right, Bottom, Left, Top);
}