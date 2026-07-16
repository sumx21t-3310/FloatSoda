using FloatSoda.Abstractions.Geometries;
using FloatSoda.Core;
using SkiaSharp;

namespace FloatSoda.RenderObjects;

public abstract class RenderBox : RenderObject
{
    public override SKSize Size { get; protected set; } = SKSize.Empty;
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
}
