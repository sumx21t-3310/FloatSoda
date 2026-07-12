namespace FloatSoda.OVR;

public readonly record struct PointerData(float X, float Y, PointerChange Change);

public enum PointerChange
{
    Up,
    Down,
    Hover
}
