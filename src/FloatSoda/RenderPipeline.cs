using FloatSoda.Common.Geometries;
using FloatSoda.Common.Layer;
using FloatSoda.Render;
using SkiaSharp;

namespace FloatSoda;

public class RenderPipeline
{
    public RenderView? RenderView { get; set; }


    public void FlushLayout() => RenderView?.PerformLayout();

    public void FlushPaint()
    {
        if (RenderView == null) return;
        var root = RenderView.Layer;
        var context = new PaintingContext(root, SKRect.Create(Offset.Zero, RenderView.Size));

        RenderView?.Paint(context, Offset.Zero);
        context.StopRecordingIfNeeded();
    }
    
    public ILayer Publish() => RenderView?.Layer.Clone() ?? throw new Exception("RenderView is null");
}