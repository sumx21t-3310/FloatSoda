using FloatSoda.Common.Geometries;
using FloatSoda.Common.Layer;
using FloatSoda.Core;
using FloatSoda.Geometrics;
using SkiaSharp;

namespace FloatSoda.RenderObjects;

public class RenderView : RenderObject, IHasSingleChildRenderObject
{
    public override SKSize Size { get; protected set; }

    private readonly SingleChildContainer<RenderBox> _child;

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

    public RenderView(float width, float height)
    {
        Size = new SKSize(width, height);
        _child = new SingleChildContainer<RenderBox>(this);
    }

    public override bool IsRepaintBoundary => true;

    public override void PerformLayout() => Child?.Layout(BoxConstraints.Tight(Size));


    public override void Paint(PaintingContext context, Offset offset) => Child?.Paint(context, offset);

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

    public void PrepareInitialFrame()
    {
        ScheduleInitialLayout();
        ScheduleInitialPaint(new TransformLayer());
    }

    private void ScheduleInitialPaint(ContainerLayer rootLayer)
    {
        Layer = rootLayer;
        Owner?.NodesNeedingPaint.Add(this);
    }

    private void ScheduleInitialLayout()
    {
        RelayoutBoundary = this;
        Owner?.NodesNeedingLayout.Add(this);
    }
}