using FloatSoda.OVR.Exceptions.Resources;

namespace FloatSoda.OVR.Exceptions;

/// <summary>
/// Thrown when an OpenVR screenshot operation (<see cref="EVRScreenshotError"/>) fails.
/// </summary>
public class VRScreenshotException(string message, EVRScreenshotError errorCode)
    : OpenVRSystemException<EVRScreenshotError>(message, errorCode)
{
    /// <summary>Creates an exception with a message derived from <paramref name="errorCode"/>.</summary>
    public VRScreenshotException(EVRScreenshotError errorCode)
        : this(GetMessage(errorCode), errorCode) { }

    private static string GetMessage(EVRScreenshotError error) => error switch
    {
        EVRScreenshotError.None => ExceptionMessages.VRScreenshotException_None,
        EVRScreenshotError.RequestFailed => ExceptionMessages.VRScreenshotException_RequestFailed,
        EVRScreenshotError.IncompatibleVersion => ExceptionMessages.VRScreenshotException_IncompatibleVersion,
        EVRScreenshotError.NotFound => ExceptionMessages.VRScreenshotException_NotFound,
        EVRScreenshotError.BufferTooSmall => ExceptionMessages.VRScreenshotException_BufferTooSmall,
        EVRScreenshotError.ScreenshotAlreadyInProgress => ExceptionMessages
            .VRScreenshotException_ScreenshotAlreadyInProgress,

        _ => throw new ArgumentOutOfRangeException(nameof(error), error, null)
    };

    /// <summary>Throws a <see cref="VRScreenshotException"/> if <paramref name="error"/> indicates a failure.</summary>
    public static void ThrowIfError(EVRScreenshotError error)
    {
        if (error == EVRScreenshotError.None) return;

        throw new VRScreenshotException(error);
    }
}