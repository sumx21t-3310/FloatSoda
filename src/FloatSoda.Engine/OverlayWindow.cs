using FloatSoda.Common.Layer;
using FloatSoda.OVR;
using FloatSoda.OVR.Overlay;

namespace FloatSoda.Engine;

public interface IWindow : IDisposable
{
    string Key { get; }
    ILayer Root { get; set; }

    void Update();
}

public class OverlayWindow(IOverlay overlay, Renderer renderer) : IWindow
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

    public void Dispose() => Overlay.Dispose();
}