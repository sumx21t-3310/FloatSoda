using FloatSoda.Abstractions.Geometries;

namespace FloatSoda.Abstractions.Input;

/// <summary>ポインタのボタン種別。</summary>
public enum PointerButton
{
    /// <summary>左ボタン（VRレーザーのトリガーを含む主ボタン）。</summary>
    Left,

    /// <summary>中ボタン。</summary>
    Middle,

    /// <summary>右ボタン。</summary>
    Right
}

/// <summary>生ポインタイベントの種別。</summary>
public enum RawPointerKind
{
    /// <summary>ポインタが入力面に入った。</summary>
    Enter,

    /// <summary>ポインタが入力面から出た。</summary>
    Leave,

    /// <summary>ポインタが移動した。</summary>
    Move,

    /// <summary>ボタンが押された。</summary>
    ButtonDown,

    /// <summary>ボタンが離された。</summary>
    ButtonUp,

    /// <summary>スクロールした。</summary>
    Scroll
}

/// <summary>
/// 入力基盤（GLFW / OpenVR）から届いた 1 件の生ポインタイベント。
/// 意味づけ（Add/Remove の合成や PointerId の採番）は行わず、観測した事実のみを運びます。
/// </summary>
/// <param name="Kind">イベントの種別。</param>
/// <param name="Position">入力面ローカルのピクセル座標（原点左上）。全イベントで有効。</param>
/// <param name="Button"><see cref="RawPointerKind.ButtonDown"/> / <see cref="RawPointerKind.ButtonUp"/> のときのボタン種別。</param>
/// <param name="ScrollDelta"><see cref="RawPointerKind.Scroll"/> のときのスクロール量。</param>
public readonly record struct RawPointerEvent(
    RawPointerKind Kind,
    Offset Position,
    PointerButton Button = PointerButton.Left,
    Offset ScrollDelta = default);

/// <summary>
/// ウィンドウ／オーバーレイ 1 枚に対応する生ポインタ入力源。
/// イベントは基盤をポンプしているスレッドで発火します（発火スレッドは実装依存）。
/// UI スレッドへのマーシャリングは購読側の責務です。
/// </summary>
public interface IRawPointerSource : IDisposable
{
    /// <summary>生ポインタイベントの通知。</summary>
    event Action<RawPointerEvent>? OnPointerEvent;
}
