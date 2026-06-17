using FloatSoda.Render;

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

    public void FlushLayout() => RenderView.PerformLayout();

    public void FlushPaint()
    {
        var dirtyNodes = NodesNeedingPaint.ToList();

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