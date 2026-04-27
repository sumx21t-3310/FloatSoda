using SkiaSharp;

namespace FloatSoda.Render;

public class RenderContext(SKPaint paint, SKCanvas canvas) : IDisposable
{
    public SKPaint Paint => paint;
    public SKCanvas Canvas => canvas;

    public static RenderContext Create(SKSurface surface) => new(new SKPaint(), surface.Canvas);

    public void Dispose()
    {
        paint.Dispose();
    }
}