using FloatSoda.Common.Geometries;
using SkiaSharp;

namespace FloatSoda.Render.Painting;

public class RenderColoredBox : RenderProxyBox
{
    public SKColor Color { get; init; } = SKColors.Black;

    public override void Paint(PaintingContext context, Offset offset)
    {
        if (!Size.IsEmpty)
        {
            context.Canvas.DrawRect(SKRect.Create(offset, Size), new SKPaint { Color = Color });
        }

        Child?.Paint(context, offset);
    }
}