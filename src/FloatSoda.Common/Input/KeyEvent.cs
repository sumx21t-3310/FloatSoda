namespace FloatSoda.Common.Input;

public readonly record struct KeyEvent(int PhysicalKey, int LogicalKey, string? Character, KeyEventPhase Phase);

public enum KeyEventPhase
{
    Down,
    Up,
    Repeat
}