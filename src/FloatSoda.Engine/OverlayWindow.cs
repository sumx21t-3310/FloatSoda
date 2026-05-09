using FloatSoda.Common.Layer;
using FloatSoda.OVR;
using OVRSharp;
using Valve.VR;

namespace FloatSoda.Engine;

public interface IWindow : IDisposable
{
    string Key { get; }
    ILayer Root { get; set; }
    public bool Visible { get; set; }
    public float Width { get; set; }
    void Update();
}

public class OverlayWindow(string key, string name, bool isDashboard, Renderer renderer) : IWindow
{
    private readonly Overlay _overlay = new(key, name, isDashboard);
    public TrackingTransform Transform { get; } = new();

    public string Key => _overlay.Key;
    public ILayer? Root { get; set; }

    public bool IsDashboard => _overlay.IsDashboardOverlay;

    public bool Visible
    {
        get;
        set
        {
            if (field == value) return;

            field = value;
            if (field)
            {
                _overlay.Show();
            }
            else
            {
                _overlay.Hide();
            }
        }
    }

    public float Width
    {
        get => _overlay.WidthInMeters;
        set => _overlay.WidthInMeters = value;
    }

    public void Update()
    {
        Transform.Update(_overlay.Handle);

        if (Root == null)
        {
            Visible = false;
            return;
        }

        renderer.Render(Root);

        var texture = new Texture_t
        {
            handle = renderer.GetTextureHandle(),
            eType = ETextureType.OpenGL,
            eColorSpace = EColorSpace.Auto,
        };

        _overlay.SetTexture(texture);
    }


    public void Dispose() => _overlay.Destroy();
}