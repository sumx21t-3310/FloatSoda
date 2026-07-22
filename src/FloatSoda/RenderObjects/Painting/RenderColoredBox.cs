using FloatSoda.Abstractions.Geometries;
using FloatSoda.Gesture;
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

        if (Child is not null) context.PaintChild(Child, offset);
    }


    public override bool HitTest(HitTestResult result, Offset position)
    {
        if (!Size.Contains(position)) return false;
        
        var hitTarget = HitTestChildren(result, position) || HitTestSelf(position);

        if (hitTarget)
        {
            result.Add(new HitTestEntry(this));
        }

        return hitTarget;
    }

    public override bool HitTestSelf(Offset position) => true;
}
