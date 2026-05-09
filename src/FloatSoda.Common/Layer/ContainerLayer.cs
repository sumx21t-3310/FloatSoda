using FloatSoda.Common.Geometries;

namespace FloatSoda.Common.Layer;

public class ContainerLayer : ILayer
{
    public List<ILayer> Children { get; } = [];
    
    public bool HasChildren => Children.Count != 0;

    public Rect PaintBounds { get; private set; }

    public virtual void Layout(LayerContext context) => PaintBounds = LayoutChildren(context);

    protected Rect LayoutChildren(LayerContext context)
    {
        var bounds = Rect.Zero;

        foreach (var child in Children)
        {
            child.Layout(context);
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