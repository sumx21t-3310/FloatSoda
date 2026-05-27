using FloatSoda.Common.Geometries;
using FloatSoda.Geometrics;

namespace FloatSoda.Render;

public class RenderPipeline
{
    public RenderView? RenderView { get; set; }


    public void FlushLayout() => RenderView?.PerformLayout();

    public void FlushPaint()
    {
        if (RenderView == null) return;
        var root = RenderView.Layer;
        var context = new PaintingContext(root, RenderView.Size.And(Offset.Zero));
        RenderView?.Paint(context, Offset.Zero);
        context.StopRecordingIfNeeded();
    }
}