using FloatSoda.OVR.Exceptions.Resources;

namespace FloatSoda.OVR.Exceptions;

/// <summary>
/// Thrown when an OpenVR input (action system) operation (<see cref="EVRInputError"/>) fails.
/// </summary>
public class VRInputException(string message, EVRInputError errorCode)
    : OpenVRSystemException<EVRInputError>(message, errorCode)
{
    /// <summary>Creates an exception with a message derived from <paramref name="errorCode"/>.</summary>
    public VRInputException(EVRInputError errorCode) : this(GetMessage(errorCode), errorCode)
    {
    }

    private static string GetMessage(EVRInputError error) => error switch
    {
        EVRInputError.NameNotFound => ExceptionMessages.VRInputException_NameNotFound,
        EVRInputError.WrongType => ExceptionMessages.VRInputException_WrongType,
        EVRInputError.InvalidHandle => ExceptionMessages.VRInputException_InvalidHandle,
        EVRInputError.InvalidParam => ExceptionMessages.VRInputException_InvalidParam,
        EVRInputError.NoSteam => ExceptionMessages.VRInputException_NoSteam,
        EVRInputError.MaxCapacityReached => ExceptionMessages.VRInputException_MaxCapacityReached,
        EVRInputError.IPCError => ExceptionMessages.VRInputException_IPCError,
        EVRInputError.NoActiveActionSet => ExceptionMessages.VRInputException_NoActiveActionSet,
        EVRInputError.InvalidDevice => ExceptionMessages.VRInputException_InvalidDevice,
        EVRInputError.InvalidSkeleton => ExceptionMessages.VRInputException_InvalidSkeleton,
        EVRInputError.InvalidBoneCount => ExceptionMessages.VRInputException_InvalidBoneCount,
        EVRInputError.InvalidCompressedData => ExceptionMessages.VRInputException_InvalidCompressedData,
        EVRInputError.NoData => ExceptionMessages.VRInputException_NoData,
        EVRInputError.BufferTooSmall => ExceptionMessages.VRInputException_BufferTooSmall,
        EVRInputError.MismatchedActionManifest => ExceptionMessages.VRInputException_MismatchedActionManifest,
        EVRInputError.MissingSkeletonData => ExceptionMessages.VRInputException_MissingSkeletonData,
        EVRInputError.InvalidBoneIndex => ExceptionMessages.VRInputException_InvalidBoneIndex,
        EVRInputError.InvalidPriority => ExceptionMessages.VRInputException_InvalidPriority,
        EVRInputError.PermissionDenied => ExceptionMessages.VRInputException_PermissionDenied,
        EVRInputError.InvalidRenderModel => ExceptionMessages.VRInputException_InvalidRenderModel,
        _ => string.Format(ExceptionMessages.VRInputException_UnexpectedError, error)
    };

    /// <summary>Throws a <see cref="VRInputException"/> if <paramref name="error"/> indicates a failure.</summary>
    public static void ThrowIfError(EVRInputError error)
    {
        if (error == EVRInputError.None) return;
        throw new VRInputException(error);
    }
}
