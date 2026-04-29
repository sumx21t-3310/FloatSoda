using FloatSoda.Engine.Render;
using SkiaSharp;

namespace FloatSoda.Engine.Painting;

public class PaintLayer : ILayer
{
    public Size Size { get; init; }
    public Offset Offset { get; init; }
    public SKColor Color { get; init; } = SKColors.Transparent;

    public void Layout(RenderContext context, ILayer parent)
    {
    }

    public void Paint(RenderContext context, ILayer parent)
    {
        context.Paint.Color = Color;
        context.Paint.Style = SKPaintStyle.Fill;

        context.Canvas.DrawRect(0, 0, Size.Width, Size.Height, context.Paint);
    }
}