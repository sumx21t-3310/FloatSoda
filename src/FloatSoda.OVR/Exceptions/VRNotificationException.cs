using FloatSoda.OVR.Exceptions.Resources;

namespace FloatSoda.OVR.Exceptions;

/// <summary>
/// Thrown when an OpenVR notification operation (<see cref="EVRNotificationError"/>) fails.
/// </summary>
public class VRNotificationException(string message, EVRNotificationError errorCode)
    : OpenVRSystemException<EVRNotificationError>(message, errorCode)
{
    /// <summary>Creates an exception with a message derived from <paramref name="errorCode"/>.</summary>
    public VRNotificationException(EVRNotificationError errorCode)
        : this(GetMessage(errorCode), errorCode)
    {
    }

    private static string GetMessage(EVRNotificationError error) => error switch
    {
        EVRNotificationError.InvalidNotificationId => ExceptionMessages.VRNotificationException_InvalidNotificationId,
        EVRNotificationError.NotificationQueueFull => ExceptionMessages.VRNotificationException_NotificationQueueFull,
        EVRNotificationError.InvalidOverlayHandle => ExceptionMessages.VRNotificationException_InvalidOverlayHandle,
        EVRNotificationError.SystemWithUserValueAlreadyExists => ExceptionMessages.VRNotificationException_SystemWithUserValueAlreadyExists,
        _ => throw new ArgumentOutOfRangeException(nameof(error), error, null)
    };

    /// <summary>Throws a <see cref="VRNotificationException"/> if <paramref name="error"/> indicates a failure.</summary>
    public static void ThrowIfError(EVRNotificationError error)
    {
        if (error == EVRNotificationError.OK) return;

        throw new VRNotificationException(error);
    }
}
