namespace FloatSoda.OVR.Exceptions;

public class VRInputException(string message, EVRInputError errorCode)
    : OpenVRSystemException<EVRInputError>(message, errorCode)
{
    public VRInputException(EVRInputError errorCode) : this(GetMessage(errorCode), errorCode)
    {
    }

    private static string GetMessage(EVRInputError error) => error switch
    {
        EVRInputError.NameNotFound => "指定された入力アクションまたはパスが見つかりません。",
        EVRInputError.WrongType => "要求された入力タイプが期待された型と一致しません。",
        EVRInputError.InvalidHandle => "無効な入力ハンドルが指定されました。",
        EVRInputError.InvalidParam => "無効なパラメータが指定されました。",
        EVRInputError.NoSteam => "Steam または SteamVR が利用できません。",
        EVRInputError.MaxCapacityReached => "内部リソースの最大数に達しました。",
        EVRInputError.IPCError => "SteamVR との IPC 通信中にエラーが発生しました。",
        EVRInputError.NoActiveActionSet => "有効なアクションセットが設定されていません。",
        EVRInputError.InvalidDevice => "無効なデバイスが指定されました。",
        EVRInputError.InvalidSkeleton => "スケルトンデータが無効です。",
        EVRInputError.InvalidBoneCount => "ボーン数が正しくありません。",
        EVRInputError.InvalidCompressedData => "圧縮データが無効です。",
        EVRInputError.NoData => "利用可能な入力データが存在しません。",
        EVRInputError.BufferTooSmall => "指定されたバッファサイズが不足しています。",
        EVRInputError.MismatchedActionManifest => "現在読み込まれている Action Manifest が一致しません。",
        EVRInputError.MissingSkeletonData => "必要なスケルトンデータが不足しています。",
        EVRInputError.InvalidBoneIndex => "無効なボーンインデックスが指定されました。",
        EVRInputError.InvalidPriority => "無効な優先度が指定されました。",
        EVRInputError.PermissionDenied => "要求された操作へのアクセスが拒否されました。",
        EVRInputError.InvalidRenderModel => "無効なレンダーモデルが指定されました。",
        _ => $"予期しない入力エラーが発生しました: {error}"
    };

    public static void ThrowIfError(EVRInputError error)
    {
        if (error == EVRInputError.None) return;
        throw new VRInputException(error);
    }
}