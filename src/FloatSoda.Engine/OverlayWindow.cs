using FloatSoda.Common.Geometries;
using FloatSoda.Common.Layer;
using FloatSoda.OVR;
using FloatSoda.OVR.Overlay;

namespace FloatSoda.Engine;

public class OverlayWindow(IOverlay overlay, Renderer renderer, Dpm dpm) : IWindow
{
    public IOverlay Overlay { get; } = overlay;

    public string Key => Overlay.Identity.Key;

    public ILayer? Root { get; set; }

    public void Update()
    {
        if (Root == null)
        {
            return;
        }

        renderer.Render(Root);

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
    public void Resize(int width, int height)
    {
        renderer.Resize(width, height);
        Overlay.WidthInMeters.Value = dpm.ToMeters(width);
    }

    public void Dispose() => Overlay.Dispose();
}