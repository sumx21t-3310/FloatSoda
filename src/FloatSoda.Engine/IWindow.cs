using FloatSoda.Engine.Painting;
using FloatSoda.Engine.Render;

namespace FloatSoda.Engine;

public interface IWindow : IDisposable
{
    ILayer Root { get; set; }
    public bool Visible { get; set; }
    public float Width { get; set; }
    void Update();
}