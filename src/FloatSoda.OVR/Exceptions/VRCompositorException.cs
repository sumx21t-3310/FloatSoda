namespace FloatSoda.OVR.Exceptions;

public class VRCompositorException(string message, EVRCompositorError errorCode)
    : OpenVRSystemException<EVRCompositorError>(message, errorCode)
{
    public VRCompositorException(EVRCompositorError error) : this(GetMessage(error), error)
    {
    }

    private static string GetMessage(EVRCompositorError error) => error switch
    {
        EVRCompositorError.RequestFailed => "コンポジターへの要求に失敗しました。",
        EVRCompositorError.IncompatibleVersion => "アプリケーションのコンポジター バージョンがランタイムと互換性がありません。",
        EVRCompositorError.DoNotHaveFocus => "アプリケーションが現在フォーカスを持っていません。",
        EVRCompositorError.InvalidTexture => "送信されたテクスチャが無効です。",
        EVRCompositorError.IsNotSceneApplication => "アプリケーションがシーンアプリケーションとして登録されていません。",
        EVRCompositorError.TextureIsOnWrongDevice => "テクスチャが別のグラフィックスデバイス上で作成されています。",
        EVRCompositorError.TextureUsesUnsupportedFormat => "テクスチャ形式がコンポジターでサポートされていません。",
        EVRCompositorError.SharedTexturesNotSupported => "共有テクスチャはこのシステムでサポートされていません。",
        EVRCompositorError.IndexOutOfRange => "テクスチャ インデックスが範囲外です。",
        EVRCompositorError.AlreadySubmitted => "この目に対するフレームは既に送信されています。",
        EVRCompositorError.InvalidBounds => "送信されたテクスチャ境界が無効です。",
        EVRCompositorError.AlreadySet => "コンポジター プロパティは既に設定されています。",
        _ => throw new ArgumentOutOfRangeException(nameof(error), error, null)
    };

    public static void ThrowIfError(EVRCompositorError error)
    {
        if (error == EVRCompositorError.None)
            return;

        throw new VRCompositorException(error);
    }
}