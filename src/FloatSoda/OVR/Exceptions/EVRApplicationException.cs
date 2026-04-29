using Valve.VR;

namespace FloatSoda.OVR.Exceptions;

public class EVRApplicationException(string message, EVRApplicationError errorCode) : OVRException<EVRApplicationError>(message, errorCode);

public static class EVRApplicationExceptionValidator
{
    public static void ThrowIfError(this EVRApplicationError error)
    {
        if (error == EVRApplicationError.None) return;

        throw error switch
        {
            EVRApplicationError.AppKeyAlreadyExists => new EVRApplicationException("指定されたアプリケーションキーは既に存在します。", error),
            EVRApplicationError.NoManifest => new EVRApplicationException("アプリケーションマニフェストが見つかりません。", error),
            EVRApplicationError.NoApplication => new EVRApplicationException("指定されたアプリケーションが見つかりません。", error),
            EVRApplicationError.InvalidIndex => new EVRApplicationException("インデックスが無効です。", error),
            EVRApplicationError.UnknownApplication => new EVRApplicationException("未知のアプリケーションです。", error),
            EVRApplicationError.IPCFailed => new EVRApplicationException("プロセス間通信(IPC)に失敗しました。", error),
            EVRApplicationError.ApplicationAlreadyRunning => new EVRApplicationException("アプリケーションは既に実行中です。", error),
            EVRApplicationError.InvalidManifest => new EVRApplicationException("マニフェストファイルが不正です。", error),
            EVRApplicationError.InvalidApplication => new EVRApplicationException("アプリケーションの設定が無効です。", error),
            EVRApplicationError.LaunchFailed => new EVRApplicationException("アプリケーションの起動に失敗しました。", error),
            EVRApplicationError.ApplicationAlreadyStarting => new EVRApplicationException("アプリケーションは既に起動処理中です。", error),
            EVRApplicationError.LaunchInProgress => new EVRApplicationException("別の起動処理が進行中です。", error),
            EVRApplicationError.OldApplicationQuitting => new EVRApplicationException("以前のアプリケーションが終了処理中のため、起動できません。", error),
            EVRApplicationError.TransitionAborted => new EVRApplicationException("アプリケーションの遷移が中断されました。", error),
            EVRApplicationError.IsTemplate => new EVRApplicationException("指定されたアプリはテンプレートであり、直接起動できません。", error),
            EVRApplicationError.SteamVRIsExiting => new EVRApplicationException("SteamVRが終了処理中のため、要求を完了できません。", error),
            EVRApplicationError.BufferTooSmall => new EVRApplicationException("バッファサイズが不足しています。", error),
            EVRApplicationError.PropertyNotSet => new EVRApplicationException("プロパティが設定されていません。", error),
            EVRApplicationError.UnknownProperty => new EVRApplicationException("未知のプロパティが要求されました。", error),
            EVRApplicationError.InvalidParameter => new EVRApplicationException("パラメータが無効です。", error),
            EVRApplicationError.NotImplemented => new EVRApplicationException("要求された機能は実装されていません。", error),
            
            _ => new EVRApplicationException($"予期しないアプリケーションエラーが発生しました: {error}", error)
        };
    }
}