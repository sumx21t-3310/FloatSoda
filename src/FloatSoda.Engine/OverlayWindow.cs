using FloatSoda.Abstractions.Engine;
using FloatSoda.Abstractions.Geometries;
using FloatSoda.Rendering.Layers;
using FloatSoda.OVR;
using FloatSoda.OVR.Overlay;

namespace FloatSoda.Engine;

public class OverlayWindow(IOverlay overlay, Renderer renderer, Dpm dpm) : IEngineWindow
{
    public IOverlay Overlay { get; } = overlay;

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
    /// </summary>
    public void Resize(SkiaSharp.SKSizeI size)
    {
        renderer.Resize(size.Width, size.Height);
        Overlay.WidthInMeters.Value = dpm.ToMeters(size.Width);
    }

    public void Dispose() => Overlay.Dispose();
}
