using FloatSoda.OVR.Exceptions.Resources;

namespace FloatSoda.OVR.Exceptions;

/// <summary>
/// Thrown when an OpenVR overlay operation (<see cref="EVROverlayError"/>) fails.
/// </summary>
public class VROverlayException(string message, EVROverlayError errorCode)
    : OpenVRSystemException<EVROverlayError>(message, errorCode)
{
    /// <summary>Creates an exception with a message derived from <paramref name="errorCode"/>.</summary>
    public VROverlayException(EVROverlayError errorCode) : this(GetMessage(errorCode), errorCode)
    {
    }

    private static string GetMessage(EVROverlayError error)
    {
        return error switch
        {
            EVROverlayError.UnknownOverlay => ExceptionMessages.VROverlayException_UnknownOverlay,
            EVROverlayError.InvalidHandle => ExceptionMessages.VROverlayException_InvalidHandle,
            EVROverlayError.PermissionDenied => ExceptionMessages.VROverlayException_PermissionDenied,
            EVROverlayError.OverlayLimitExceeded => ExceptionMessages.VROverlayException_OverlayLimitExceeded,
            EVROverlayError.WrongVisibilityType => ExceptionMessages.VROverlayException_WrongVisibilityType,
            EVROverlayError.KeyTooLong => ExceptionMessages.VROverlayException_KeyTooLong,
            EVROverlayError.NameTooLong => ExceptionMessages.VROverlayException_NameTooLong,
            EVROverlayError.KeyInUse => ExceptionMessages.VROverlayException_KeyInUse,
            EVROverlayError.WrongTransformType => ExceptionMessages.VROverlayException_WrongTransformType,
            EVROverlayError.InvalidTrackedDevice => ExceptionMessages.VROverlayException_InvalidTrackedDevice,
            EVROverlayError.InvalidParameter => ExceptionMessages.VROverlayException_InvalidParameter,
            EVROverlayError.ThumbnailCantBeDestroyed => ExceptionMessages.VROverlayException_ThumbnailCantBeDestroyed,
            EVROverlayError.ArrayTooSmall => ExceptionMessages.VROverlayException_ArrayTooSmall,
            EVROverlayError.RequestFailed => ExceptionMessages.VROverlayException_RequestFailed,
            EVROverlayError.InvalidTexture => ExceptionMessages.VROverlayException_InvalidTexture,
            EVROverlayError.UnableToLoadFile => ExceptionMessages.VROverlayException_UnableToLoadFile,
            EVROverlayError.KeyboardAlreadyInUse => ExceptionMessages.VROverlayException_KeyboardAlreadyInUse,
            EVROverlayError.NoNeighbor => ExceptionMessages.VROverlayException_NoNeighbor,
            EVROverlayError.TooManyMaskPrimitives => ExceptionMessages.VROverlayException_TooManyMaskPrimitives,
            EVROverlayError.BadMaskPrimitive => ExceptionMessages.VROverlayException_BadMaskPrimitive,
            EVROverlayError.TextureAlreadyLocked => ExceptionMessages.VROverlayException_TextureAlreadyLocked,
            EVROverlayError.TextureLockCapacityReached => ExceptionMessages.VROverlayException_TextureLockCapacityReached,
            EVROverlayError.TextureNotLocked => ExceptionMessages.VROverlayException_TextureNotLocked,
            _ => string.Format(ExceptionMessages.VROverlayException_UnexpectedError, error)
        };
    }

    /// <summary>Throws a <see cref="VROverlayException"/> if <paramref name="error"/> indicates a failure.</summary>
    public static void ThrowIfError(EVROverlayError error)
    {
        if (error == EVROverlayError.None) return;

        throw new VROverlayException(error);
    }

    /// <summary>Throws a <see cref="VROverlayException"/> if <paramref name="handle"/> is an invalid overlay handle.</summary>
    public static void ThrowIfInvalidHandle(ulong handle)
    {
        if (handle == OpenVR.k_ulOverlayHandleInvalid) throw new VROverlayException(EVROverlayError.InvalidHandle);
    }
}
