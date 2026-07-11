using FloatSoda.OVR.Exceptions.Resources;

namespace FloatSoda.OVR.Exceptions;

/// <summary>
/// Thrown when an OpenVR tracked device property operation (<see cref="ETrackedPropertyError"/>) fails.
/// </summary>
public class TrackedPropertyException(string message, ETrackedPropertyError errorCode)
    : OpenVRSystemException<ETrackedPropertyError>(message, errorCode)
{
    private static string GetMessage(ETrackedPropertyError error) => error switch
    {
        ETrackedPropertyError.TrackedProp_Success => ExceptionMessages.TrackedPropertyException_TrackedProp_Success,
        ETrackedPropertyError.TrackedProp_WrongDataType => ExceptionMessages.TrackedPropertyException_TrackedProp_WrongDataType,
        ETrackedPropertyError.TrackedProp_WrongDeviceClass => ExceptionMessages.TrackedPropertyException_TrackedProp_WrongDeviceClass,
        ETrackedPropertyError.TrackedProp_BufferTooSmall => ExceptionMessages.TrackedPropertyException_TrackedProp_BufferTooSmall,
        ETrackedPropertyError.TrackedProp_UnknownProperty => ExceptionMessages.TrackedPropertyException_TrackedProp_UnknownProperty,
        ETrackedPropertyError.TrackedProp_InvalidDevice => ExceptionMessages.TrackedPropertyException_TrackedProp_InvalidDevice,
        ETrackedPropertyError.TrackedProp_CouldNotContactServer => ExceptionMessages.TrackedPropertyException_TrackedProp_CouldNotContactServer,
        ETrackedPropertyError.TrackedProp_ValueNotProvidedByDevice => ExceptionMessages.TrackedPropertyException_TrackedProp_ValueNotProvidedByDevice,
        ETrackedPropertyError.TrackedProp_StringExceedsMaximumLength => ExceptionMessages.TrackedPropertyException_TrackedProp_StringExceedsMaximumLength,
        ETrackedPropertyError.TrackedProp_NotYetAvailable => ExceptionMessages.TrackedPropertyException_TrackedProp_NotYetAvailable,
        ETrackedPropertyError.TrackedProp_PermissionDenied => ExceptionMessages.TrackedPropertyException_TrackedProp_PermissionDenied,
        ETrackedPropertyError.TrackedProp_InvalidOperation => ExceptionMessages.TrackedPropertyException_TrackedProp_InvalidOperation,
        ETrackedPropertyError.TrackedProp_CannotWriteToWildcards => ExceptionMessages.TrackedPropertyException_TrackedProp_CannotWriteToWildcards,
        ETrackedPropertyError.TrackedProp_IPCReadFailure => ExceptionMessages.TrackedPropertyException_TrackedProp_IPCReadFailure,
        ETrackedPropertyError.TrackedProp_OutOfMemory => ExceptionMessages.TrackedPropertyException_TrackedProp_OutOfMemory,
        ETrackedPropertyError.TrackedProp_InvalidContainer => ExceptionMessages.TrackedPropertyException_TrackedProp_InvalidContainer,
        _ => throw new ArgumentOutOfRangeException(nameof(error), error, null)
    };

    /// <summary>Throws a <see cref="TrackedPropertyException"/> if <paramref name="error"/> indicates a failure.</summary>
    public static void ThrowIfError(ETrackedPropertyError error)
    {
        if (error == ETrackedPropertyError.TrackedProp_Success) return;

        throw new TrackedPropertyException(GetMessage(error), error);
    }
}
