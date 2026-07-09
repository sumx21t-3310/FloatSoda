using System.Numerics;
using FloatSoda.Core;
using FloatSoda.Elements;
using FloatSoda.OVR.Overlay;
using SkiaSharp;

namespace FloatSoda.Widgets;

/// <summary>
/// ウィンドウ設定（タイトル・ルート制約）を担うルートウィジェットの基底。
/// ウィンドウサイズの決定方法（<see cref="Size"/> による Tight / Loosen 制約）はこの基底が責務を持ち、
/// 表示先（OpenVR オーバーレイ / 将来のデスクトップウィンドウ）は派生型が決めます。
/// <see cref="FloatSodaApp.CreateWindow"/> に渡して使用します。
/// </summary>
public abstract record WindowWidget : InheritedWidget
{
    /// <summary>
    /// ウィンドウに表示されるタイトル。SteamVR 上の表示名（ダッシュボードタブ名など）になります。
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// ウィンドウの固定サイズ。null の場合は <see cref="Child"/> のレイアウト結果サイズに
    /// ウィンドウサイズが追従します（Loosen）。指定するとそのサイズで子をレイアウトします（Tight）。
    /// </summary>
    public SKSize? Size { get; init; }

    /// <summary>
    /// 具象型によらず <see cref="WindowWidget"/> として lookup できるよう、基底型で登録する。
    /// </summary>
    public sealed override Type ScopeType => typeof(WindowWidget);

    /// <summary>
    /// 最も近い祖先の <see cref="WindowWidget"/> を取得し、依存関係を登録します。
    /// </summary>
    public static WindowWidget Of(IBuildContext context) =>
        context.DependOnInheritedWidgetOfExactType<WindowWidget>() ??
        throw new InvalidOperationException("WindowWidgetが祖先に見つかりません。");

    public override bool UpdateShouldNotify(InheritedWidget oldWidget) => !Equals(oldWidget, this);

    public override Element CreateElement() => new WindowElement { Widget = this };
}

/// <summary>
/// OpenVR オーバーレイとして表示されるウィンドウの基底。
/// <see cref="IOverlay"/> の生成責務とオーバーレイキーの管理はこの階層が持ちます
/// （将来のデスクトップウィンドウはこの知識を持ちません）。
/// </summary>
public abstract record OverlayWindow : WindowWidget
{
    /// <summary>
    /// 最も近い祖先の <see cref="OverlayWindow"/> を取得し、依存関係を登録します。
    /// ルートがオーバーレイ以外のウィンドウの場合は例外をスローします。
    /// </summary>
    public new static OverlayWindow Of(IBuildContext context) =>
        WindowWidget.Of(context) as OverlayWindow ??
        throw new InvalidOperationException("OverlayWindowが祖先に見つかりません。");

    /// <summary>
    /// このウィンドウ定義に対応する <see cref="IOverlay"/> を生成します。レンダースレッド上で呼ばれます。
    /// </summary>
    internal abstract IOverlay CreateOverlay();

    /// <summary>
    /// OpenVR のオーバーレイキー。「エントリアセンブリ名 + <see cref="WindowWidget.Title"/> のスネークケース」から生成されます。
    /// </summary>
    private protected string Key => WindowKeyGenerator.GenerateKey(Title);
}

/// <summary>
/// SteamVR ダッシュボードに表示されるウィンドウ。
/// </summary>
public record DashboardWindow : OverlayWindow
{
    internal override IOverlay CreateOverlay()
        => new DashboardOverlay(new DashboardOverlayIdentity(Key, Title));
}

/// <summary>
/// ワールド空間の指定座標に固定されるウィンドウ。
/// </summary>
public record WorldSpaceWindow : OverlayWindow
{
    /// <summary>ワールド空間上の配置座標。</summary>
    public required Vector3 Position { get; init; }

    /// <summary>オーバーレイの回転。既定は無回転。</summary>
    public Quaternion Rotation { get; init; } = Quaternion.Identity;

    internal override IOverlay CreateOverlay()
    {
        var overlay = new WorldSpaceOverlay(new OverlayIdentity(Key, Title));
        overlay.Transform.Position = Position;
        overlay.Transform.Rotation = Rotation;
        overlay.Visibility.Show();
        return overlay;
    }
}

/// <summary>
/// 指定したトラッキングデバイスに追従するウィンドウ。
/// </summary>
public record DeviceTrackedWindow : OverlayWindow
{
    /// <summary>追従対象のトラッキングデバイス。</summary>
    public required TrackedDevice Target { get; init; }

    /// <summary>デバイス基準の位置オフセット。既定はゼロ。</summary>
    public Vector3 Offset { get; init; } = Vector3.Zero;

    /// <summary>オーバーレイの回転。既定は無回転。</summary>
    public Quaternion Rotation { get; init; } = Quaternion.Identity;

    internal override IOverlay CreateOverlay()
    {
        var overlay = new DeviceTrackedOverlay(new OverlayIdentity(Key, Title));
        overlay.Transform.Target = Target;
        overlay.Transform.Position = Offset;
        overlay.Transform.Rotation = Rotation;
        overlay.Visibility.Show();
        return overlay;
    }
}
