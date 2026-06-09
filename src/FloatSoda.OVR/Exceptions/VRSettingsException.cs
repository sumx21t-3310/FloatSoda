namespace FloatSoda.OVR.Exceptions;

public class VRSettingsException(string message, EVRSettingsError errorCode)
    : OpenVRSystemException<EVRSettingsError>(message, errorCode)
{
    public VRSettingsException(EVRSettingsError errorCode) : this(GetMessage(errorCode), errorCode)
    {
    }

    private static string GetMessage(EVRSettingsError error) => error switch
    {
        EVRSettingsError.None => "設定エラーは発生していません。",
        EVRSettingsError.IPCFailed => "設定システムとのIPC通信に失敗しました。",
        EVRSettingsError.WriteFailed => "設定の書き込みに失敗しました。",
        EVRSettingsError.ReadFailed => "設定の読み込みに失敗しました。",
        EVRSettingsError.JsonParseFailed => "設定ファイルのJSON解析に失敗しました。",
        EVRSettingsError.UnsetSettingHasNoDefault => "設定値が未設定であり、既定値も存在しません。",
        _ => throw new ArgumentOutOfRangeException(nameof(error), error, null)
    };

    public static void ThrowIfError(EVRSettingsError error)
    {
        if (error == EVRSettingsError.None) return;

        throw new VRSettingsException(error);
    }
}