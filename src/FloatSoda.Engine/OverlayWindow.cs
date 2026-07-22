using FloatSoda.Abstractions.Engine;
using FloatSoda.Abstractions.Geometries;
using FloatSoda.Abstractions.Input;
using FloatSoda.Rendering.Layers;
using FloatSoda.OVR;
using FloatSoda.OVR.Overlay;

namespace FloatSoda.Engine;

public class OverlayWindow : IEngineWindow
{
    private readonly Renderer renderer;
    private readonly Dpm dpm;

    public IOverlay Overlay { get; }

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

    public void Dispose()
    {
        PointerSource?.Dispose();
        Overlay.Dispose();
    }
}
