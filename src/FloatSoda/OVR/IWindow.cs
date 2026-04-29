using FloatSoda.Render;

namespace FloatSoda.OVR;

public interface IWindow : IDisposable
{
    Element Root { get; set; }
    public bool Visible { get; set; }
    public float Width { get; set; }
    void Update();
}