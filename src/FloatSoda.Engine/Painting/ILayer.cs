using FloatSoda.Engine.Render;

namespace FloatSoda.Engine.Painting;

public interface ILayer
{
    void Layout(RenderContext context, ILayer parent);
    void Paint(RenderContext context, ILayer parent);
}