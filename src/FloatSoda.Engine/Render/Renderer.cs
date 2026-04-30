using FloatSoda.Engine.Painting;

namespace FloatSoda.Engine.Render;

public class Renderer(GLView glView) : IDisposable
{
    private readonly RenderContext _renderContext = RenderContext.Create(glView.Surface);

    public IntPtr GetTextureHandle() => glView.TextureHandle;

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