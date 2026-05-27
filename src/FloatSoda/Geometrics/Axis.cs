namespace FloatSoda.Geometrics;

public enum Axis
{
    Horizontal,
    Vertical
}

public static class AxisExtension
{
    public static Axis Flip(this Axis axis) => axis switch
    {
        Axis.Horizontal => Axis.Vertical,
        Axis.Vertical => Axis.Horizontal,
        _ => throw new ArgumentOutOfRangeException(nameof(axis), axis, null)
    };
}

public enum MainAxisAlignment
{
    Start,
    End,
    Center,
    SpaceBetween,
    SpaceAround,
    SpaceEvenly,
}

public enum CrossAxisAlignment
{
    Start,
    End,
    Center,
    Stretch,
    Baseline,
}

public enum MainAxisSize
{
    Min,
    Max,
}

public enum VerticalDirection
{
    Up,
    Down,
}