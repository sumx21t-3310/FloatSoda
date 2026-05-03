namespace FloatSoda.Engine.Layer;

public class ClipRectLayer : ContainerLayer
{
    public Rect ClipRect { get; set; }
    public Clip ClipBehavior { get; set; }


    public override void Layout(LayerContext context)
    {
        var clipPathBounds = LayoutChildren();
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

public enum Clip
{
    None,
    HardEdge,
    Antialias
}