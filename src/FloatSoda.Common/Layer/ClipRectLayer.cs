using SkiaSharp;

namespace FloatSoda.Common.Layer;

public class ClipRectLayer : ContainerLayer
{
    public SKRect ClipRect { get; set; }
    public Clip ClipBehavior { get; set; }


    public override void Layout(LayerContext context)
    {
        var clipPathBounds = LayoutChildren(context);

        if (!SKRect.Intersect(ClipRect, clipPathBounds).IsEmpty)
        {
            PaintBounds = clipPathBounds;
        }

        context.Canvas.Restore();
    }

    public override void Paint(LayerContext context)
    {
        context.Canvas.Save();
        context.Canvas.ClipRect(ClipRect, antialias: ClipBehavior == Clip.Antialias);

        base.Paint(context);
        context.Canvas.Restore();
    }
}

public class ClipRRectLayer : ContainerLayer
{
    public SKRoundRect ClipRect { get; set; }
    public Clip ClipBehavior { get; set; }


    public override void Layout(LayerContext context)
    {
        var clipPathBounds = LayoutChildren(context);

        if (!SKRect.Intersect(ClipRect.Rect, clipPathBounds).IsEmpty)
        {
            PaintBounds = clipPathBounds;
        }

        context.Canvas.Restore();
    }

    public override void Paint(LayerContext context)
    {
        context.Canvas.Save();
        context.Canvas.ClipRoundRect(ClipRect, antialias: ClipBehavior == Clip.Antialias);

        base.Paint(context);
        context.Canvas.Restore();
    }
}

public class ClipPathLayer : ContainerLayer
{
    public SKPath clipPath { get; set; }
    public Clip ClipBehavior { get; set; }

    public override void Layout(LayerContext context)
    {
        var clipPathBounds = LayoutChildren(context);
        var clipPaintBounds = PaintBounds;

        if (!SKRect.Intersect(clipPathBounds, clipPaintBounds).IsEmpty)
        {
            PaintBounds = clipPathBounds;
        }
    }

    public override void Paint(LayerContext context)
    {
        context.Canvas.Save();
        context.Canvas.ClipPath(clipPath, antialias: ClipBehavior == Clip.Antialias);
        base.Paint(context);
        context.Canvas.Restore();
    }
}

public enum Clip
{
    None,
    HardEdge,
    Antialias
}