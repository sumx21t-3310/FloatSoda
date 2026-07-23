using FloatSoda.Abstractions.Engine;
using FloatSoda.Abstractions.Geometries;
using FloatSoda.Abstractions.Input;
using FloatSoda.Rendering.Layers;
using FloatSoda.OVR;
using FloatSoda.OVR.Overlay;

namespace FloatSoda.Engine;

/// <summary>SteamVRオーバーレイを描画先とする<see cref="IEngineWindow"/>の実装です。</summary>
/// <seealso cref="IOverlay"/>
public class OverlayWindow : IEngineWindow
{
    private readonly Renderer renderer;
    private readonly Dpm dpm;

    /// <summary>このウィンドウが描画先とするOpenVRオーバーレイを取得します。</summary>
    public IOverlay Overlay { get; }

    /// <summary>指定されたオーバーレイを描画先とするウィンドウを作成します。</summary>
    /// <param name="overlay">描画先となるOpenVRオーバーレイ。</param>
    /// <param name="renderer">レイヤーツリーをテクスチャへ描画するレンダラー。</param>
    /// <param name="dpm">ピクセルサイズをオーバーレイの物理幅へ換算する際に使用する密度。</param>
    /// <remarks>
    /// <paramref name="overlay"/>がダッシュボードオーバーレイの場合、入力方式をマウスへ設定し、
    /// SteamVRのレーザーポインター入力を受け取る<see cref="PointerSource"/>を生成します。
    /// それ以外のオーバーレイでは<see cref="PointerSource"/>は<see langword="null"/>のままです。
    /// </remarks>
    public OverlayWindow(IOverlay overlay, Renderer renderer, Dpm dpm)
    {
        Overlay = overlay;
        this.renderer = renderer;
        this.dpm = dpm;

        // ダッシュボードはSteamVRのレーザーがマウスイベントとして届く。
        // それ以外のオーバーレイの入力（コントローラーレイ経路）は未実装のため当面 null。
        if (overlay is IDashboardOverlay)
        {
            overlay.Input.Method = VROverlayInputMethod.Mouse;
            overlay.Input.SetMouseScale(renderer.GLView.Size.Width, renderer.GLView.Size.Height);
            PointerSource = new OverlayMouseRawPointerSource(overlay.EventDispatcher);
        }
    }

    /// <summary>
    /// 指定されたレイヤーツリーを描画し、結果のテクスチャをオーバーレイへ反映します。
    /// </summary>
    /// <param name="layer">描画するレイヤーツリー。<see langword="null"/>は指定できません。</param>
    /// <remarks>レンダースレッド上で呼び出します。</remarks>
    public void Present(ILayer layer)
    {
        renderer.Render(layer);

        var texture = new Texture_t
        {
            handle = renderer.GetTextureHandle(),
            eType = ETextureType.OpenGL,
            eColorSpace = EColorSpace.Auto,
        };

        Overlay.Texture.FromTexture_t(texture);
    }

    /// <summary>
    /// 描画先テクスチャをリサイズし、ピクセル幅と物理密度（dots per meter）から
    /// オーバーレイの物理幅（WidthInMeters = width / dpm）を更新します。
    /// マウス入力を受けるオーバーレイではマウススケールもピクセルサイズへ追従させます。
    /// </summary>
    public void Resize(SkiaSharp.SKSizeI size)
    {
        renderer.Resize(size.Width, size.Height);
        Overlay.WidthInMeters.Value = dpm.ToMeters(size.Width);

        if (PointerSource != null)
        {
            Overlay.Input.SetMouseScale(size.Width, size.Height);
        }
    }

    /// <summary>このウィンドウの生ポインター入力源を取得します。</summary>
    /// <value>ダッシュボードオーバーレイの場合はレーザーポインター入力源。それ以外では<see langword="null"/>です。</value>
    public IRawPointerSource? PointerSource { get; }

    /// <summary>
    /// このオーバーレイ宛のOpenVRイベントをポーリングし、<see cref="PointerSource"/> へ流します。
    /// メインスレッドから毎フレーム呼びます。入力を持たないオーバーレイでは何もしません。
    /// </summary>
    public void PollInputEvents()
    {
        if (PointerSource == null) return;

        Overlay.EventDispatcher.PollEvents();
    }

    /// <summary>このウィンドウの生ポインター入力源とオーバーレイを破棄します。</summary>
    public void Dispose()
    {
        PointerSource?.Dispose();
        Overlay.Dispose();
    }
}
