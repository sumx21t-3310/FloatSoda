using FloatSoda.Abstractions.Geometries;
using FloatSoda.Abstractions.Input;
using FloatSoda.Core;
using FloatSoda.Gesture;
using SkiaSharp;

namespace FloatSoda.RenderObjects;

public abstract class RenderBox : RenderObject
{
    public override SKSize Size { get; protected set; } = SKSize.Empty;

    public virtual bool HitTest(HitTestResult result, Offset position)
    {
        if (!Size.Contains(position)) return false;

        if (!HitTestChildren(result, position) && !HitTestSelf(position)) return false;

        result.Add(new HitTestEntry(this));

        return true;
    }

    public virtual bool HitTestChildren(HitTestResult result, Offset position) => false;

    public virtual bool HitTestSelf(Offset position) => false;

    public override void HandleEvent(PointerEvent pointerEvent, HitTestEntry entry)
    {
        // do nothing
    }
}

public abstract class RenderProxyBox : RenderBox, IHasSingleChildRenderObject
{
    private readonly SingleChildContainer<RenderBox> _child;

    protected RenderProxyBox() => _child = new SingleChildContainer<RenderBox>(this);

    public RenderBox? Child
    {
        get => _child.Child;
        set => _child.Child = value;
    }

    RenderObject? IHasSingleChildRenderObject.Child
    {
        get => Child;
        set => Child = (RenderBox?)value;
    }

    public override void SetupParentData(RenderObject child) => child.ParentData = new BoxParentData();

    public override void PerformLayout()
    {
        if (Child != null)
        {
            Child.Layout(Constraints, parentUseSize: true);
            Size = Child.Size;
        }
        else
        {
            Size = Constraints.Smallest;
        }
    }

    public override void Paint(PaintingContext context, Offset offset)
    {
        if (Child != null) context.PaintChild(Child, offset);
    }

    public override void Attach(RenderPipeline? owner)
    {
        base.Attach(owner);
        _child.Attach(owner);
    }

    public override void Detach()
    {
        base.Detach();
        _child.Detach();
    }

    public override void VisitChildren(Action<RenderObject> visitor) => _child.VisitChildren(visitor);

    public override void RedepthChildren() => VisitChildren(RedepthChild);

    public override bool HitTestChildren(HitTestResult result, Offset position)
    {
        return Child?.HitTest(result, position) ?? false;
    }
}
