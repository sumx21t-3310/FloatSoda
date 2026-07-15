using FloatSoda.Abstractions.Geometries;

namespace FloatSoda.Abstractions.Input;

public enum PointerEventPhase
{
    Up,
    Down,
    Move,
    Add,
    Remove
};

public readonly record struct PointerEvent(
    int PointerId,
    PointerEventPhase Phase,
    Offset Position,
    Offset? Transform = null);