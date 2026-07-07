using FloatSoda.Common.Geometries;
using FloatSoda.Core;
using FloatSoda.Geometrics;
using FloatSoda.RenderObjects;

namespace FloatSoda.Widgets.Layout;

public record Padding : SingleChildRenderObjectWidget<RenderSiftedBox>
{
    public EdgeInsets Spacing { get; init; } = EdgeInsets.Zero;


    public override RenderSiftedBox CreateRenderObject()
    {
        return new RenderSiftedBox();
    }

    public override void UpdateRenderObject(RenderSiftedBox renderObject)
    {
        throw new NotImplementedException();
    }
}

public class RenderSiftedBox : RenderBox, IHasSingleChildRenderObject
{
    private readonly SingleChildContainer<RenderBox> _child;

    public RenderSiftedBox() => _child = new SingleChildContainer<RenderBox>(this);

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

    public override void PerformLayout()
    {
        throw new NotImplementedException();
    }

    public override void Paint(PaintingContext context, Offset offset)
    {
        var child = Child;

        if (child == null) return;

        var childParentData = child.ParentData as BoxParentData;
        child.Paint(context, offset + childParentData?.Offset ?? Offset.Zero);
    }
}

public class RenderPadding : RenderSiftedBox
{
    public required EdgeInsets Spacing { get; init; } = EdgeInsets.Zero;
}