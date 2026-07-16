using FloatSoda.OVR;

namespace FloatSoda;

/// <summary>
/// Generic Hostへ登録するFloatSodaランタイムの設定です。
/// </summary>
public sealed record FloatSodaOptions
{
    /// <summary>
    /// SteamVRがアプリケーションを識別するためのキーを取得します。
    /// </summary>
    public AppKey AppKey { get; init; } = new("FloatSoda");

    /// <summary>
    /// メインループと描画ループの目標フレームレートを取得します。
    /// </summary>
    public int TargetFrameRate { get; init; } = 60;
}
