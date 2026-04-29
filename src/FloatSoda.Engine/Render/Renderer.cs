using FloatSoda.Engine.Painting;

namespace FloatSoda.Engine.Render;

public class Renderer(GLView glView) : IDisposable
{
    public GLView GLView => glView;
    private readonly RenderContext _renderContext = RenderContext.Create(glView.Surface);

    public void Render(ILayer root)
    {
        glView.Clear();

        root.Layout(_renderContext, root);
        root.Paint(_renderContext, root);

        glView.Flush();
    }

    public void Dispose()
    {
        _renderContext.Dispose();
        glView.Dispose();
    }
}