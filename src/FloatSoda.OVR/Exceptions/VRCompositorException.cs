using FloatSoda.OVR.Exceptions.Resources;

namespace FloatSoda.OVR.Exceptions;

/// <summary>
/// Thrown when an OpenVR compositor operation (<see cref="EVRCompositorError"/>) fails.
/// </summary>
public class VRCompositorException(string message, EVRCompositorError errorCode)
    : OpenVRSystemException<EVRCompositorError>(message, errorCode)
{
    /// <summary>Creates an exception with a message derived from <paramref name="error"/>.</summary>
    public VRCompositorException(EVRCompositorError error) : this(GetMessage(error), error)
    {
    }

    private static string GetMessage(EVRCompositorError error) => error switch
    {
        EVRCompositorError.RequestFailed => ExceptionMessages.VRCompositorException_RequestFailed,
        EVRCompositorError.IncompatibleVersion => ExceptionMessages.VRCompositorException_IncompatibleVersion,
        EVRCompositorError.DoNotHaveFocus => ExceptionMessages.VRCompositorException_DoNotHaveFocus,
        EVRCompositorError.InvalidTexture => ExceptionMessages.VRCompositorException_InvalidTexture,
        EVRCompositorError.IsNotSceneApplication => ExceptionMessages.VRCompositorException_IsNotSceneApplication,
        EVRCompositorError.TextureIsOnWrongDevice => ExceptionMessages.VRCompositorException_TextureIsOnWrongDevice,
        EVRCompositorError.TextureUsesUnsupportedFormat => ExceptionMessages.VRCompositorException_TextureUsesUnsupportedFormat,
        EVRCompositorError.SharedTexturesNotSupported => ExceptionMessages.VRCompositorException_SharedTexturesNotSupported,
        EVRCompositorError.IndexOutOfRange => ExceptionMessages.VRCompositorException_IndexOutOfRange,
        EVRCompositorError.AlreadySubmitted => ExceptionMessages.VRCompositorException_AlreadySubmitted,
        EVRCompositorError.InvalidBounds => ExceptionMessages.VRCompositorException_InvalidBounds,
        EVRCompositorError.AlreadySet => ExceptionMessages.VRCompositorException_AlreadySet,
        _ => throw new ArgumentOutOfRangeException(nameof(error), error, null)
    };

    /// <summary>Throws a <see cref="VRCompositorException"/> if <paramref name="error"/> indicates a failure.</summary>
    public static void ThrowIfError(EVRCompositorError error)
    {
        if (error == EVRCompositorError.None)
            return;

        throw new VRCompositorException(error);
    }
}
