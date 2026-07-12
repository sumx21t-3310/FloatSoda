using FloatSoda.Common.Geometries;

namespace FloatSoda.Common.Input;

/// <summary>
/// ポインタを生成したデバイスの種別。Flutter の <c>PointerDeviceKind</c> に対応します。
/// </summary>
public enum PointerDeviceKind
{
    /// <summary>タッチパネル等の接触入力。</summary>
    Touch,

    /// <summary>マウス。</summary>
    Mouse,

    /// <summary>スタイラス。</summary>
    Stylus,

    /// <summary>反転スタイラス(消しゴム側)。</summary>
    InvertedStylus,

    /// <summary>トラックパッド。</summary>
    Trackpad,

    /// <summary>VR コントローラのレイキャストポインタ。</summary>
    Controller,

    /// <summary>種別不明。</summary>
    Unknown,
}

/// <summary>
/// <see cref="PointerEvent.Buttons"/> のビットマスク定数。Flutter の <c>kPrimaryButton</c> 等に対応します。
/// </summary>
public static class PointerButtons
{
    /// <summary>主ボタン(マウス左ボタン、タッチの接触、コントローラのトリガー)。</summary>
    public const int Primary = 1 << 0;

    /// <summary>副ボタン(マウス右ボタン、スタイラスの第1ボタン)。</summary>
    public const int Secondary = 1 << 1;

    /// <summary>第3ボタン(マウス中ボタン)。</summary>
    public const int Tertiary = 1 << 2;
}

/// <summary>
/// ポインタ入力イベントの基底型。Flutter の <c>PointerEvent</c> に対応し、
/// 具体的なイベント種別は派生 record(<see cref="PointerDownEvent"/> 等)で表します。
/// </summary>
public abstract record PointerEvent
{
    /// <summary>イベント発生時刻。任意の起点(通常はアプリ起動時)からの経過時間です。</summary>
    public TimeSpan TimeStamp { get; init; }

    /// <summary>ポインタの識別子。同一ポインタの Down→Move→Up の系列で同じ値を持ちます。</summary>
    public int Pointer { get; init; }

    /// <summary>ポインタを生成したデバイスの種別。</summary>
    public PointerDeviceKind Kind { get; init; } = PointerDeviceKind.Unknown;

    /// <summary>デバイスの識別子。同種デバイスが複数ある場合(左右のコントローラ等)の区別に使います。</summary>
    public int Device { get; init; }

    /// <summary>ウィンドウ座標系(論理ピクセル)でのポインタ位置。</summary>
    public Offset Position { get; init; }

    /// <summary>前回イベントからの <see cref="Position"/> の変化量。</summary>
    public Offset Delta { get; init; }

    /// <summary>押下中のボタンのビットマスク。<see cref="PointerButtons"/> の組み合わせです。</summary>
    public int Buttons { get; init; }

    /// <summary>ポインタがダウン状態(接触中・トリガー押下中)かどうか。</summary>
    public bool Down { get; init; }

    /// <summary>押下圧力。0.0(非接触)〜1.0(最大)。アナログトリガーの引き量にも使います。</summary>
    public double Pressure { get; init; }

    /// <summary>検出面からの距離。VR ではレイの原点からヒット点までの距離(メートル)に使います。</summary>
    public double Distance { get; init; }

    /// <summary>デバイス入力由来ではなく、フレームワークが補完のために合成したイベントかどうか。</summary>
    public bool Synthesized { get; init; }
}

/// <summary>ポインタがデバイスとして認識された。Flutter の <c>PointerAddedEvent</c> に対応します。</summary>
public sealed record PointerAddedEvent : PointerEvent;

/// <summary>ポインタがデバイスとして認識されなくなった。Flutter の <c>PointerRemovedEvent</c> に対応します。</summary>
public sealed record PointerRemovedEvent : PointerEvent;

/// <summary>ポインタが領域に入った。Flutter の <c>PointerEnterEvent</c> に対応します。</summary>
public sealed record PointerEnterEvent : PointerEvent;

/// <summary>ポインタが領域から出た。Flutter の <c>PointerExitEvent</c> に対応します。</summary>
public sealed record PointerExitEvent : PointerEvent;

/// <summary>ダウン状態でないポインタが移動した。Flutter の <c>PointerHoverEvent</c> に対応します。</summary>
public sealed record PointerHoverEvent : PointerEvent;

/// <summary>ポインタがダウン状態になった。Flutter の <c>PointerDownEvent</c> に対応します。</summary>
public sealed record PointerDownEvent : PointerEvent
{
    /// <summary>ダウンイベントを表すため、既定値は <see langword="true"/> です。</summary>
    public PointerDownEvent() => Down = true;
}

/// <summary>ダウン状態のポインタが移動した。Flutter の <c>PointerMoveEvent</c> に対応します。</summary>
public sealed record PointerMoveEvent : PointerEvent
{
    /// <summary>ムーブイベントはダウン中の移動を表すため、既定値は <see langword="true"/> です。</summary>
    public PointerMoveEvent() => Down = true;
}

/// <summary>ポインタのダウン状態が解除された。Flutter の <c>PointerUpEvent</c> に対応します。</summary>
public sealed record PointerUpEvent : PointerEvent;

/// <summary>ポインタ入力の系列が中断された(トラッキングロスト等)。Flutter の <c>PointerCancelEvent</c> に対応します。</summary>
public sealed record PointerCancelEvent : PointerEvent;
