using FloatSoda.Engine.Layer;

namespace FloatSoda.Engine.Render;

public class Renderer(GLView glView) : IDisposable
{
    public IntPtr GetTextureHandle() => glView.TextureHandle;

    public void Render(ILayer root)
    {
        var renderContext = LayerContext.Create(glView.Surface);

        glView.Clear();

        root.Layout(renderContext);
        root.Paint(renderContext);

        glView.Flush();
    }

    public void Dispose() => glView.Dispose();
}