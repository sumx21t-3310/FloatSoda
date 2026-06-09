namespace FloatSoda.OVR.Exceptions;

public class VRRenderModelException(string message, EVRRenderModelError errorCode)
    : OpenVRSystemException<EVRRenderModelError>(message, errorCode)
{
    private VRRenderModelException(EVRRenderModelError error)
        : this(GetMessage(error), error)
    {
    }

    private static string GetMessage(EVRRenderModelError error) => error switch
    {
        EVRRenderModelError.Loading => "レンダーモデルは現在読み込み中です。",
        EVRRenderModelError.NotSupported => "この環境ではレンダーモデル機能がサポートされていません。",
        EVRRenderModelError.InvalidArg => "無効な引数が指定されました。",
        EVRRenderModelError.InvalidModel => "レンダーモデルが無効、または破損しています。",
        EVRRenderModelError.NoShapes => "レンダーモデルに形状データが含まれていません。",
        EVRRenderModelError.MultipleShapes => "複数の形状データが検出されましたが、単一形状のみサポートされています。",
        EVRRenderModelError.TooManyVertices => "レンダーモデルの頂点数が多すぎます。",
        EVRRenderModelError.MultipleTextures => "複数のテクスチャが検出されましたが、単一テクスチャのみサポートされています。",
        EVRRenderModelError.BufferTooSmall => "バッファサイズが不足しています。",
        EVRRenderModelError.NotEnoughNormals => "法線データが不足しています。",
        EVRRenderModelError.NotEnoughTexCoords => "テクスチャ座標データが不足しています。",
        EVRRenderModelError.InvalidTexture => "テクスチャデータが無効、または破損しています。",
        _ => $"不明なレンダーモデルエラーが発生しました。 ({error})"
    };

    public static void ThrowIfError(EVRRenderModelError error)
    {
        if (error == EVRRenderModelError.None) return;

        throw new VRRenderModelException(error);
    }
}