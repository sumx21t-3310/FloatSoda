using FloatSoda.Engine.OVR.Exceptions;
using FloatSoda.Engine.Painting;
using FloatSoda.Engine.Render;
using Valve.VR;

namespace FloatSoda.Engine;

public class DashboardWindow : IWindow
{
    private readonly Renderer _renderer;
    private ulong _overlayHandle = OpenVR.k_ulOverlayHandleInvalid;
    private ulong _thumbnailHandle = OpenVR.k_ulOverlayHandleInvalid;

    public ILayer Root { get; set; }

    public DashboardWindow(string key, string name, string iconPath, Renderer renderer, ILayer root, float width = 0.5f, bool visible = false)
    {
        _renderer = renderer;
        Root = root;


        OpenVR.Overlay.CreateDashboardOverlay(key, name, ref _overlayHandle, ref _thumbnailHandle).ThrowIfError();

        OpenVR.Overlay.SetOverlayFromFile(_thumbnailHandle, iconPath).ThrowIfError();

        Width = width;
        Visible = visible;
    }

    public bool Visible
    {
        get;
        set
        {
            field = value;
            if (field)
            {
                OpenVR.Overlay.ShowOverlay(_overlayHandle).ThrowIfError();
            }
            else
            {
                OpenVR.Overlay.HideOverlay(_overlayHandle).ThrowIfError();
            }
        }
    }

    public float Width
    {
        get;
        set
        {
            field = Math.Max(0.01f, value);
            OpenVR.Overlay.SetOverlayWidthInMeters(_overlayHandle, field).ThrowIfError();
        }
    }


    public void Update()
    {
        _renderer.Render(Root);
        var texture = new Texture_t
        {
            handle = _renderer.GLView.TextureHandle,
            eType = ETextureType.OpenGL,
            eColorSpace = EColorSpace.Auto,
        };

        OpenVR.Overlay.SetOverlayTexture(_overlayHandle, ref texture).ThrowIfError();
    }


    public void Dispose()
    {
        if (_overlayHandle == OpenVR.k_ulOverlayHandleInvalid) return;

        OpenVR.Overlay.DestroyOverlay(_overlayHandle).ThrowIfError();
        _overlayHandle = OpenVR.k_ulOverlayHandleInvalid;
    }
}