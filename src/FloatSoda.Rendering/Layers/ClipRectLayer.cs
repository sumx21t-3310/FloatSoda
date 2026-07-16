using SkiaSharp;

namespace FloatSoda.Rendering.Layers;

public class ClipRectLayer(SKRect clipRect) : ContainerLayer
{
    public SKRect ClipRect { get; set; } = clipRect;
    public Clip ClipBehavior { get; set; } = Clip.Antialias;


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

    public override ILayer Clone()
    {
        var cloned = new ClipRectLayer(ClipRect)
        {
            ClipBehavior = ClipBehavior
        };

        foreach (var child in Children)
        {
            cloned.Children.Add(child.Clone());
        }

        return cloned;
    }
}

public class ClipRoundRectLayer(SKRoundRect clipRect) : ContainerLayer
{
    public SKRoundRect ClipRect { get; set; } = clipRect;
    public Clip ClipBehavior { get; set; } = Clip.Antialias;


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

    public override ILayer Clone()
    {
        var cloned = new ClipRoundRectLayer(ClipRect)
        {
            ClipBehavior = ClipBehavior
        };

        foreach (var child in Children)
        {
            cloned.Children.Add(child.Clone());
        }

        return cloned;
    }
}

public class ClipPathLayer(SKPath clipPath) : ContainerLayer
{
    public SKPath ClipPath { get; set; } = clipPath;
    public Clip ClipBehavior { get; set; } = Clip.Antialias;

    public override void Layout(LayerContext context)
    {
        var clipPathBounds = ClipPath.Bounds;
        var clipPaintBounds = LayoutChildren(context);

        if (!SKRect.Intersect(clipPathBounds, clipPaintBounds).IsEmpty)
        {
            PaintBounds = clipPathBounds;
        }
    }

    public override void Paint(LayerContext context)
    {
        context.Canvas.Save();
        context.Canvas.ClipPath(ClipPath, antialias: ClipBehavior == Clip.Antialias);
        base.Paint(context);
        context.Canvas.Restore();
    }

    public override ILayer Clone()
    {
        var cloned = new ClipPathLayer(ClipPath) { ClipBehavior = ClipBehavior };

        foreach (var child in Children)
        {
            cloned.Children.Add(child.Clone());
        }

        return cloned;
    }
}

public enum Clip
{
    None,
    HardEdge,
    Antialias
}