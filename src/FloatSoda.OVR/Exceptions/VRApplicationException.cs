namespace FloatSoda.OVR.Exceptions;

public class VRApplicationException(string message, EVRApplicationError errorCode)
    : OpenVRSystemException<EVRApplicationError>(message, errorCode)
{
    public VRApplicationException(EVRApplicationError errorCode) : this(GetMessage(errorCode), errorCode)
    {
    }


    private static string GetMessage(EVRApplicationError error)
    {
        return error switch
        {
            EVRApplicationError.AppKeyAlreadyExists => "指定されたアプリケーションキーは既に存在します。",
            EVRApplicationError.NoManifest => "アプリケーションマニフェストが見つかりません。",
            EVRApplicationError.NoApplication => "指定されたアプリケーションが見つかりません。",
            EVRApplicationError.InvalidIndex => "インデックスが無効です。",
            EVRApplicationError.UnknownApplication => "未知のアプリケーションです。",
            EVRApplicationError.IPCFailed => "プロセス間通信(IPC)に失敗しました。",
            EVRApplicationError.ApplicationAlreadyRunning => "アプリケーションは既に実行中です。",
            EVRApplicationError.InvalidManifest => "マニフェストファイルが不正です。",
            EVRApplicationError.InvalidApplication => "アプリケーションの設定が無効です。",
            EVRApplicationError.LaunchFailed => "アプリケーションの起動に失敗しました。",
            EVRApplicationError.ApplicationAlreadyStarting => "アプリケーションは既に起動処理中です。",
            EVRApplicationError.LaunchInProgress => "別の起動処理が進行中です。",
            EVRApplicationError.OldApplicationQuitting => "以前のアプリケーションが終了処理中のため、起動できません。",

            EVRApplicationError.TransitionAborted => "アプリケーションの遷移が中断されました。",
            EVRApplicationError.IsTemplate => "指定されたアプリはテンプレートであり、直接起動できません。",
            EVRApplicationError.SteamVRIsExiting => "SteamVRが終了処理中のため、要求を完了できません。",
            EVRApplicationError.BufferTooSmall => "バッファサイズが不足しています。",
            EVRApplicationError.PropertyNotSet => "プロパティが設定されていません。",
            EVRApplicationError.UnknownProperty => "未知のプロパティが要求されました。",
            EVRApplicationError.InvalidParameter => "パラメータが無効です。",
            EVRApplicationError.NotImplemented => "要求された機能は実装されていません。",

            _ => $"予期しないアプリケーションエラーが発生しました: {error}"
        };
    }

    public static void ThrowIfError(EVRApplicationError error)
    {
        if (error == EVRApplicationError.None) return;

        throw new VRApplicationException(error);
    }
}