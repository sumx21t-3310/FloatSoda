using FloatSoda.Common.Geometries;
using FloatSoda.Geometrics;
using SkiaSharp;

namespace FloatSoda.Render;

public class RenderColoredBox : RenderProxyBox
{
    public SKColor Color { get; init; } = SKColors.Black;

    public override void Paint(PaintingContext context, Offset offset)
    {
        if (!Size.IsEmpty)
        {
            context.Canvas.DrawRect(Size.And(offset), new SKPaint { Color = Color });
        }

        Child?.Paint(context, offset);
    }
}