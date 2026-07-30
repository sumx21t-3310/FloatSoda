using FloatSoda.Abstractions.Geometries;
using FloatSoda.Abstractions.Input;
using FloatSoda.Gesture;
using FloatSoda.RenderObjects;
using SkiaSharp;

namespace FloatSoda.Widgets.Paint;

internal record Opacity : SingleChildRenderObjectWidget<RenderOpacity>
{
    public override RenderOpacity CreateRenderObject()
    {
        throw new NotImplementedException();
    }
}

internal class RenderOpacity : RenderObject
{
    public override SKSize Size { get; protected set; }

    public override void PerformLayout()
    {
        throw new NotImplementedException();
    }

    public override void Paint(PaintingContext context, Offset offset)
    {
        throw new NotImplementedException();
    }

    public override void HandleEvent(PointerEvent pointerEvent, HitTestEntry entry)
    {
        throw new NotImplementedException();
    }
}
