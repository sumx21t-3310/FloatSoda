using FloatSoda.Engine.OVR.Exceptions;
using FloatSoda.Engine.Painting;
using FloatSoda.Engine.Render;
using Valve.VR;

namespace FloatSoda.Engine;

public class FloatingWindow : IWindow
{
    private readonly Renderer _renderer;
    private ulong _overlayHandle = OpenVR.k_ulOverlayHandleInvalid;

    public string Key { get; }
    public ILayer Root { get; set; }

    public TrackingTransform Transform { get; }

    public FloatingWindow(string key, string name, Renderer renderer, ILayer root, float width = 0.5f, bool visible = true)
    {
        Key = key;

        Root = root;

        OpenVR.Overlay.CreateOverlay(key, name, ref _overlayHandle).ThrowIfError();

        Width = width;
        Visible = visible;
        Transform = new TrackingTransform();
        _renderer = renderer;
    }

    public bool Visible
    {
        get;
        set
        {
            if (field == value) return;

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
            if (Math.Abs(field - value) < float.Epsilon) return;

            field = Math.Max(0.01f, value);
            OpenVR.Overlay.SetOverlayWidthInMeters(_overlayHandle, field).ThrowIfError();
        }
    }

    public void Update()
    {
        Transform.Update(_overlayHandle);
        _renderer.Render(Root);

        var texture = new Texture_t
        {
            handle = _renderer.GetTextureHandle(),
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