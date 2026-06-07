using FloatSoda.Common.Geometries;
using FloatSoda.Common.Layer;
using FloatSoda.Geometrics;
using FloatSoda.Render;

namespace FloatSoda;

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
    
    public ILayer Publish() => RenderView?.Layer.Clone() ?? throw new Exception("RenderView is null");
}