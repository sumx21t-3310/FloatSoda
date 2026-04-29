using Valve.VR;

namespace FloatSoda.OVR.Exceptions;

/// <summary>
/// OpenVRオーバーレイ操作に関する例外のベースクラス
/// </summary>
public class VrOverlayException(EVROverlayError error, string message) : OVRException<EVROverlayError>(message, error);

public static class VrOverlayValidator
{
    /// <summary>
    /// EVROverlayErrorを評価し、エラーがある場合は適切な例外をスローします。
    /// </summary>
    /// <param name="error">OpenVR APIから返されたエラーコード</param>
    public static void ThrowIfError(this EVROverlayError error)
    {
        if (error == EVROverlayError.None) return;

        // すべて VrOverlayException を継承させたメッセージ、または標準例外をラップしたものをスロー
        throw error switch
        {
            EVROverlayError.UnknownOverlay => new VrOverlayException(error, "指定されたオーバーレイが見つかりません。"),
            EVROverlayError.InvalidHandle => new VrOverlayException(error, "オーバーレイハンドルが無効です。"),
            EVROverlayError.PermissionDenied => new VrOverlayException(error, "このオーバーレイ操作を実行する権限がありません。"),
            EVROverlayError.OverlayLimitExceeded => new VrOverlayException(error, "VRオーバーレイの最大数(128個等)に達しました。"),
            EVROverlayError.WrongVisibilityType => new VrOverlayException(error, "この操作には対応していない表示タイプです。"),
            EVROverlayError.KeyTooLong => new VrOverlayException(error, "オーバーレイのキーが長すぎます。"),
            EVROverlayError.NameTooLong => new VrOverlayException(error, "オーバーレイの名前が長すぎます。"),
            EVROverlayError.KeyInUse => new VrOverlayException(error, "このキー(文字列ID)は既に別のオーバーレイで使用されています。"),
            EVROverlayError.WrongTransformType => new VrOverlayException(error, "このオーバーレイでは、要求された座標変換タイプ(Absolute/TrackedDevice等)はサポートされていません。"),
            EVROverlayError.InvalidTrackedDevice => new VrOverlayException(error, "指定されたトラックデバイスのインデックスが無効、または接続されていません。"),
            EVROverlayError.InvalidParameter => new VrOverlayException(error, "引数(パラメータ)が不正です。"),
            EVROverlayError.ThumbnailCantBeDestroyed => new VrOverlayException(error, "サムネイル・オーバーレイを単体で破棄することはできません。"),
            EVROverlayError.ArrayTooSmall => new VrOverlayException(error, "提供されたバッファサイズが不足しています。"),
            EVROverlayError.RequestFailed => new VrOverlayException(error, "OpenVRリクエストが失敗しました。"),
            EVROverlayError.InvalidTexture => new VrOverlayException(error, "テクスチャハンドルまたはポインタが無効です。"),
            EVROverlayError.UnableToLoadFile => new VrOverlayException(error, "ファイル(PNG/マニフェスト等)を読み込めませんでした。パスを確認してください。"),
            EVROverlayError.KeyboardAlreadyInUse => new VrOverlayException(error, "VRシステムキーボードは既に他のプロセスで使用されています。"),
            EVROverlayError.NoNeighbor => new VrOverlayException(error, "隣接オーバーレイが見つかりません。"),
            EVROverlayError.TooManyMaskPrimitives => new VrOverlayException(error, "マスクプリミティブの制限数を超えました。"),
            EVROverlayError.BadMaskPrimitive => new VrOverlayException(error, "マスクプリミティブの形式が不正です。"),
            EVROverlayError.TextureAlreadyLocked => new VrOverlayException(error, "テクスチャは既にロックされています。"),
            EVROverlayError.TextureLockCapacityReached => new VrOverlayException(error, "テクスチャロックのキャパシティに達しました。"),
            EVROverlayError.TextureNotLocked => new VrOverlayException(error, "操作前にテクスチャをロックする必要があります。"),

            _ => new VrOverlayException(error, $"予期しないエラーが発生しました: {error}")
        };
    }
}