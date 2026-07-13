using FloatSoda.OVR.Exceptions.Resources;

namespace FloatSoda.OVR.Exceptions;

/// <summary>
/// Thrown when an OpenVR application management operation (<see cref="EVRApplicationError"/>) fails.
/// </summary>
public class VRApplicationException(string message, EVRApplicationError errorCode)
    : OpenVRSystemException<EVRApplicationError>(message, errorCode)
{
    /// <summary>Creates an exception with a message derived from <paramref name="errorCode"/>.</summary>
    public VRApplicationException(EVRApplicationError errorCode) : this(GetMessage(errorCode), errorCode)
    {
    }


    private static string GetMessage(EVRApplicationError error)
    {
        return error switch
        {
            EVRApplicationError.AppKeyAlreadyExists => ExceptionMessages.VRApplicationException_AppKeyAlreadyExists,
            EVRApplicationError.NoManifest => ExceptionMessages.VRApplicationException_NoManifest,
            EVRApplicationError.NoApplication => ExceptionMessages.VRApplicationException_NoApplication,
            EVRApplicationError.InvalidIndex => ExceptionMessages.VRApplicationException_InvalidIndex,
            EVRApplicationError.UnknownApplication => ExceptionMessages.VRApplicationException_UnknownApplication,
            EVRApplicationError.IPCFailed => ExceptionMessages.VRApplicationException_IPCFailed,
            EVRApplicationError.ApplicationAlreadyRunning => ExceptionMessages.VRApplicationException_ApplicationAlreadyRunning,
            EVRApplicationError.InvalidManifest => ExceptionMessages.VRApplicationException_InvalidManifest,
            EVRApplicationError.InvalidApplication => ExceptionMessages.VRApplicationException_InvalidApplication,
            EVRApplicationError.LaunchFailed => ExceptionMessages.VRApplicationException_LaunchFailed,
            EVRApplicationError.ApplicationAlreadyStarting => ExceptionMessages.VRApplicationException_ApplicationAlreadyStarting,
            EVRApplicationError.LaunchInProgress => ExceptionMessages.VRApplicationException_LaunchInProgress,
            EVRApplicationError.OldApplicationQuitting => ExceptionMessages.VRApplicationException_OldApplicationQuitting,

            EVRApplicationError.TransitionAborted => ExceptionMessages.VRApplicationException_TransitionAborted,
            EVRApplicationError.IsTemplate => ExceptionMessages.VRApplicationException_IsTemplate,
            EVRApplicationError.SteamVRIsExiting => ExceptionMessages.VRApplicationException_SteamVRIsExiting,
            EVRApplicationError.WaitingForChaperone => ExceptionMessages.VRApplicationException_WaitingForChaperone,
            EVRApplicationError.BufferTooSmall => ExceptionMessages.VRApplicationException_BufferTooSmall,
            EVRApplicationError.PropertyNotSet => ExceptionMessages.VRApplicationException_PropertyNotSet,
            EVRApplicationError.UnknownProperty => ExceptionMessages.VRApplicationException_UnknownProperty,
            EVRApplicationError.InvalidParameter => ExceptionMessages.VRApplicationException_InvalidParameter,
            EVRApplicationError.NotImplemented => ExceptionMessages.VRApplicationException_NotImplemented,

            _ => string.Format(ExceptionMessages.VRApplicationException_UnexpectedError, error)
        };
    }

    /// <summary>Throws a <see cref="VRApplicationException"/> if <paramref name="error"/> indicates a failure.</summary>
    public static void ThrowIfError(EVRApplicationError error)
    {
        if (error == EVRApplicationError.None) return;

        throw new VRApplicationException(error);
    }
}
