using SkiaSharp;

namespace FloatSoda.Common.Layer;

public class ContainerLayer : ILayer
{
    public List<ILayer> Children { get; } = [];

    public bool HasChildren => Children.Count != 0;

    public SKRect PaintBounds { get; protected set; }

    public virtual void Layout(LayerContext context) => PaintBounds = LayoutChildren(context);

    protected SKRect LayoutChildren(LayerContext context)
    {
        var bounds = SKRect.Empty;

        foreach (var child in Children)
        {
            child.Layout(context);
            bounds.Union(child.PaintBounds);
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

    public virtual ILayer Clone()
    {
        var cloned = new ContainerLayer();

        foreach (var child in Children)
        {
            cloned.Children.Add(child.Clone());
        }

        return cloned;
    }
}