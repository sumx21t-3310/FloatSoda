using FloatSoda.OVR;
using FloatSoda.OVR.Input;

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

    /// <summary>
    /// アプリが使用するアクション入力の定義を取得します。
    /// 1つ以上登録すると、アクションマニフェストが自動生成されてSteamVRへ登録され、
    /// 各<see cref="InputAction{T}"/>の値とイベントが毎フレーム更新されます。
    /// </summary>
    public IReadOnlyList<InputActionMap> InputActionMaps { get; init; } = [];
}
