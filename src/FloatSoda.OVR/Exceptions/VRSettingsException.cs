using FloatSoda.OVR.Exceptions.Resources;

namespace FloatSoda.OVR.Exceptions;

/// <summary>
/// Thrown when an OpenVR settings operation (<see cref="EVRSettingsError"/>) fails.
/// </summary>
public class VRSettingsException(string message, EVRSettingsError errorCode)
    : OpenVRSystemException<EVRSettingsError>(message, errorCode)
{
    /// <summary>Creates an exception with a message derived from <paramref name="errorCode"/>.</summary>
    public VRSettingsException(EVRSettingsError errorCode) : this(GetMessage(errorCode), errorCode)
    {
    }

    private static string GetMessage(EVRSettingsError error) => error switch
    {
        EVRSettingsError.None => ExceptionMessages.VRSettingsException_None,
        EVRSettingsError.IPCFailed => ExceptionMessages.VRSettingsException_IPCFailed,
        EVRSettingsError.WriteFailed => ExceptionMessages.VRSettingsException_WriteFailed,
        EVRSettingsError.ReadFailed => ExceptionMessages.VRSettingsException_ReadFailed,
        EVRSettingsError.JsonParseFailed => ExceptionMessages.VRSettingsException_JsonParseFailed,
        EVRSettingsError.UnsetSettingHasNoDefault => ExceptionMessages.VRSettingsException_UnsetSettingHasNoDefault,
        _ => throw new ArgumentOutOfRangeException(nameof(error), error, null)
    };

    /// <summary>Throws a <see cref="VRSettingsException"/> if <paramref name="error"/> indicates a failure.</summary>
    public static void ThrowIfError(EVRSettingsError error)
    {
        if (error == EVRSettingsError.None) return;

        throw new VRSettingsException(error);
    }
}
