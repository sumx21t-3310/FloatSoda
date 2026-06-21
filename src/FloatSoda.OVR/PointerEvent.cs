namespace FloatSoda.OVR;

public readonly record struct PointerEvent(float X, float Y, PointerEventPhase EventPhase);

public enum PointerEventPhase
{
    Up,
    Down,
    Hover
}