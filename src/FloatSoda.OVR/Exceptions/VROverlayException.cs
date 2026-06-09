namespace FloatSoda.OVR.Exceptions;

public class VROverlayException(string message, EVROverlayError errorCode)
    : OpenVRSystemException<EVROverlayError>(message, errorCode)
{
    public VROverlayException(EVROverlayError errorCode) : this(GetMessage(errorCode), errorCode)
    {
    }

    private static string GetMessage(EVROverlayError error)
    {
        return error switch
        {
            EVROverlayError.UnknownOverlay => "指定されたオーバーレイが見つかりません。",
            EVROverlayError.InvalidHandle => "オーバーレイハンドルが無効です。",
            EVROverlayError.PermissionDenied => "このオーバーレイ操作を実行する権限がありません。",
            EVROverlayError.OverlayLimitExceeded => "VRオーバーレイの最大数(128個等)に達しました。",
            EVROverlayError.WrongVisibilityType => "この操作には対応していない表示タイプです。",
            EVROverlayError.KeyTooLong => "オーバーレイのキーが長すぎます。",
            EVROverlayError.NameTooLong => "オーバーレイの名前が長すぎます。",
            EVROverlayError.KeyInUse => "このキー(文字列ID)は既に別のオーバーレイで使用されています。",
            EVROverlayError.WrongTransformType => "このオーバーレイでは、要求された座標変換タイプ(Absolute/TrackedDevice等)はサポートされていません。",
            EVROverlayError.InvalidTrackedDevice => "指定されたトラックデバイスのインデックスが無効、または接続されていません。",
            EVROverlayError.InvalidParameter => "引数(パラメータ)が不正です。",
            EVROverlayError.ThumbnailCantBeDestroyed => "サムネイル・オーバーレイを単体で破棄することはできません。",
            EVROverlayError.ArrayTooSmall => "提供されたバッファサイズが不足しています。",
            EVROverlayError.RequestFailed => "OpenVRリクエストが失敗しました。",
            EVROverlayError.InvalidTexture => "テクスチャハンドルまたはポインタが無効です。",
            EVROverlayError.UnableToLoadFile => "ファイル(PNG/マニフェスト等)を読み込めませんでした。パスを確認してください。",
            EVROverlayError.KeyboardAlreadyInUse => "VRシステムキーボードは既に他のプロセスで使用されています。",
            EVROverlayError.NoNeighbor => "隣接オーバーレイが見つかりません。",
            EVROverlayError.TooManyMaskPrimitives => "マスクプリミティブの制限数を超えました。",
            EVROverlayError.BadMaskPrimitive => "マスクプリミティブの形式が不正です。",
            EVROverlayError.TextureAlreadyLocked => "テクスチャは既にロックされています。",
            EVROverlayError.TextureLockCapacityReached => "テクスチャロックのキャパシティに達しました。",
            EVROverlayError.TextureNotLocked => "操作前にテクスチャをロックする必要があります。",
            _ => $"予期しないエラーが発生しました: {error}"
        };
    }

    public static void ThrowIfError(EVROverlayError error)
    {
        if (error == EVROverlayError.None) return;

        throw new VROverlayException(error);
    }

    public static void ThrowIfInvalidHandle(ulong handle)
    {
        if (handle == OpenVR.k_ulOverlayHandleInvalid) throw new VROverlayException(EVROverlayError.InvalidHandle);
    }
}