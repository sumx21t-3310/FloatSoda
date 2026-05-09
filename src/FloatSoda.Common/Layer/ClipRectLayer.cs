using FloatSoda.Common.Geometries;

namespace FloatSoda.Common.Layer;

public class ClipRectLayer : ContainerLayer
{
    public Rect ClipRect { get; set; }
    public Clip ClipBehavior { get; set; }


    public override void Layout(LayerContext context)
    {
        var clipPathBounds = LayoutChildren(context);

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

public class ClipRRectLayer : ClipRectLayer
{
    public new RRect ClipRect { get; set; }


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
    public override void Layout(LayerContext context)
    {
        base.Layout(context);
    }

    public override void Paint(LayerContext context)
    {
        base.Paint(context);
    }
}

public enum Clip
{
    None,
    HardEdge,
    Antialias
}