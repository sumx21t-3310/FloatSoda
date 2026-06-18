using FloatSoda.Common.Layer;

namespace FloatSoda.Engine;

public class Renderer : IDisposable
{
    public required GLView GLView { get; init; }
    public IntPtr GetTextureHandle() => GLView.TextureHandle;

    public void Render(ILayer root)
    {
        var renderContext = LayerContext.Create(GLView.Surface);

        GLView.Clear();

        root.Layout(renderContext);
        root.Paint(renderContext);

        GLView.Flush();
    }

    public void Dispose() => GLView.Dispose();
}