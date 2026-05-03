namespace FloatSoda.Engine.Layer;

public class ContainerLayer : ILayer
{
    public List<ILayer> Children { get; } = [];

    public Rect PaintBounds { get; private set; }

    public virtual void Layout(LayerContext context) => PaintBounds = LayoutChildren();

    protected Rect LayoutChildren()
    {
        var bounds = new Rect();

        foreach (var child in Children)
        {
            bounds = bounds.Union(child.PaintBounds);
        }

        return bounds;
    }

    public virtual void Paint(LayerContext context)
    {
        foreach (var child in Children)
        {
            child.Paint(context);
        }
    }
}