namespace FloatSoda.OVR.Input;

/// <summary>
/// アクション入力のグループです。OpenVRのアクションセット(<c>/actions/{name}</c>)に対応します。
/// <see cref="Enabled"/> でグループ単位の有効/無効を切り替えられます。
/// </summary>
public sealed class InputActionMap
{
    /// <summary>
    /// アクションセット名を取得します(例: <c>main</c>)。
    /// マニフェスト上のフルパスは <c>/actions/{name}</c> になります。
    /// </summary>
    public required string Name { get; init; }

    /// <summary>このマップに属するアクションの一覧を取得します。</summary>
    public required IReadOnlyList<InputAction> Actions { get; init; }

    /// <summary>
    /// このマップを入力更新の対象に含めるかを取得または設定します。
    /// 無効化するとSteamVRはこのアクションセットのバインドを解放します。
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>IVRInputのアクションセットハンドル。<see cref="VRInputUpdater"/> が解決します。</summary>
    internal ulong Handle { get; set; }
}
