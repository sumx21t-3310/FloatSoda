using FloatSoda.Engine.Render;

namespace FloatSoda.Engine.Painting;

public class ContainerLayer : ILayer
{
    public List<ILayer> Children { get; } = [];

    public bool NeedsLayoutUpdate { get; protected set; } = true;

    public void MarkNeedsLayoutUpdate()
    {
        if (NeedsLayoutUpdate) return;
        NeedsLayoutUpdate = true;
    }

    public virtual void Layout(RenderContext context, ILayer parent)
    {
        if (!NeedsLayoutUpdate) return;

        OnLayout(context, parent);

        foreach (var child in Children)
        {
            child.Layout(context, this);
        }


        NeedsLayoutUpdate = false;
    }

    public virtual void Paint(RenderContext context, ILayer parent)
    {
        OnPaint(context, parent);

        foreach (var child in Children)
        {
            child.Paint(context, this);
        }
    }

    protected virtual void OnLayout(RenderContext context, ILayer parent)
    {
    }

    protected virtual void OnPaint(RenderContext context, ILayer parent)
    {
    }
}