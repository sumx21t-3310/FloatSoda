using FloatSoda.Abstractions.Geometries;
using SkiaSharp;

namespace FloatSoda.RenderObjects.Painting;

public class RenderColoredBox : RenderProxyBox
{
    public SKColor Color
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            MarkNeedsPaint();
        }
    } = SKColors.Black;

    public override void Paint(PaintingContext context, Offset offset)
    {
        if (!Size.IsEmpty)
        {
            context.Canvas.DrawRect(SKRect.Create(offset, Size), new SKPaint { Color = Color });
        }

        if (Child != null) context.PaintChild(Child, offset);
    }
}