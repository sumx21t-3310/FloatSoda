using SkiaSharp;

namespace FloatSoda.Render;

public class Renderer(GLView glView) : IDisposable
{
    public GLView GLView => glView;
    private readonly RenderContext _renderContext = new(new SKPaint(), glView.Surface.Canvas);

    public void Render(Element element)
    {
        glView.Clear();
        
        element.Draw(_renderContext);
        
        glView.Flush();
    }

    public void Dispose()
    {
        _renderContext.Dispose();
        glView.Dispose();
    }
}