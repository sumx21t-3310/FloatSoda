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

    /// <summary>描画先の GLView を指定サイズにリサイズします。GL 呼び出しのためレンダースレッド上で呼びます。</summary>
    public void Resize(int width, int height) => GLView.Resize(width, height);

    public void Dispose() => GLView.Dispose();
}