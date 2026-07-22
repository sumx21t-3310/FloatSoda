namespace FloatSoda.OVR.Input;

/// <summary>
/// アクションに対する物理入力の初期割り当ての宣言です。
/// これはSteamVRへ提案する初期値であり、実際のバインディングの決定権はSteamVR側
/// (ユーザーのリバインド)にあります。実行時に読み返すことはできません。
/// </summary>
public sealed record DefaultBinding
{
    /// <summary>この割り当てを適用するコントローラー種別を取得します。</summary>
    public required ControllerType Controller { get; init; }

    /// <summary>
    /// OpenVR正規の入力パスを取得します(例: <c>/user/hand/right/input/trigger/click</c>)。
    /// SteamVRのバインディングUIに表示される表記と一致します。
    /// </summary>
    public required string Path { get; init; }
}
