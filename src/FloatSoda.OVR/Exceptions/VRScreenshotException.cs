namespace FloatSoda.OVR.Exceptions;

public class VRScreenshotException(string message, EVRScreenshotError errorCode)
    : OpenVRSystemException<EVRScreenshotError>(message, errorCode)
{
    public VRScreenshotException(EVRScreenshotError errorCode)
        : this(GetMessage(errorCode), errorCode)
    {
    }

    private static string GetMessage(EVRScreenshotError error) => error switch
    {
        EVRScreenshotError.None => "エラーは発生していません。",
        EVRScreenshotError.RequestFailed => "スクリーンショットのリクエストに失敗しました。",
        EVRScreenshotError.IncompatibleVersion => "スクリーンショットAPIのバージョンに互換性がありません。",
        EVRScreenshotError.NotFound => "指定されたスクリーンショットが見つかりませんでした。",
        EVRScreenshotError.BufferTooSmall => "渡されたバッファサイズが不足しています。",
        EVRScreenshotError.ScreenshotAlreadyInProgress => "別のスクリーンショット処理が既に進行中です。",

        _ => throw new ArgumentOutOfRangeException(nameof(error), error, null)
    };

    public static void ThrowIfError(EVRScreenshotError error)
    {
        if (error == EVRScreenshotError.None) return;

        throw new VRScreenshotException(error);
    }
}