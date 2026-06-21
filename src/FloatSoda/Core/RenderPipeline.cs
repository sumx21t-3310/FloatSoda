using FloatSoda.RenderObjects;

namespace FloatSoda.Core;

public class RenderPipeline
{
    public required Action OnNeedVisualUpdate { get; init; }

    public required RenderView RenderView
    {
        get;
        set
        {
            value.Attach(this);
            field = value;
        }
    }

    public List<RenderObject> NodesNeedingPaint { get; } = [];
    public List<RenderObject> NodesNeedingLayout { get; } = [];

    public void FlushLayout()
    {
        while (NodesNeedingLayout.Count != 0)
        {
            var dirtyNodes = NodesNeedingLayout.OrderBy(x => x.Depth).ToList();

            NodesNeedingLayout.Clear();

            foreach (var node in dirtyNodes)
            {
                if (node.NeedsLayout && node.Owner == this)
                {
                    node.LayoutWithoutResize();
                }
            }
        }
    }

    public void FlushPaint()
    {
        var dirtyNodes = NodesNeedingPaint.OrderBy(x => x.Depth).ToList();

        NodesNeedingPaint.Clear();

        foreach (var node in dirtyNodes)
        {
            if (node.NeedsPaint && node.Owner == this)
            {
                PaintingContext.RepaintCompositedChild(node);
            }
        }
    }

    public void RequestVisualUpdate() => OnNeedVisualUpdate();
}