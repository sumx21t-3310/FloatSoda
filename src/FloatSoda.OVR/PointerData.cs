namespace FloatSoda.OVR;

/// <summary>Describes a pointer event in normalized overlay texture coordinates.</summary>
/// <param name="X">The horizontal texture coordinate.</param>
/// <param name="Y">The vertical texture coordinate.</param>
/// <param name="Change">The pointer phase observed during this poll.</param>
public readonly record struct PointerData(float X, float Y, PointerChange Change);

/// <summary>Identifies how the primary pointer button changed.</summary>
public enum PointerChange
{
    /// <summary>The primary button was released during this poll.</summary>
    Up,

    /// <summary>The primary button was pressed during this poll.</summary>
    Down,

    /// <summary>The pointer is hovering and the primary button did not change.</summary>
    Hover
}
