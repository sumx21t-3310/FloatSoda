using FloatSoda.Common.Geometries;
using FloatSoda.Render;
using SkiaSharp;

namespace FloatSoda;

public class RenderPipeline
{
    public RenderView? RenderView { get; set; }

    public bool NeedsRebuild { get; private set; } = true;

    public void FlushLayout() => RenderView?.PerformLayout();

    public void FlushPaint()
    {
        if (RenderView == null) return;
        NeedsRebuild = false;
        var root = RenderView.Layer;
        var context = new PaintingContext(root, SKRect.Create(Offset.Zero, RenderView.Size));

        RenderView?.Paint(context, Offset.Zero);
        context.StopRecordingIfNeeded();
    }
}