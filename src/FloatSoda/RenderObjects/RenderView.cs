using FloatSoda.Common.Geometries;
using FloatSoda.Common.Layer;
using FloatSoda.Core;
using FloatSoda.Geometrics;
using SkiaSharp;

namespace FloatSoda.RenderObjects;

public class RenderView(float width, float height) : RenderObject, IHasSingleChildRenderObject<RenderBox>
{
    public override SKSize Size { get; protected set; } = new(width, height);

    public RenderBox? Child { get; set; }
    public RenderObject ThisRef => this;

    public override bool IsRepaintBoundary => true;

    public override void PerformLayout() => Child?.Layout(BoxConstraints.Tight(Size));


    public override void Paint(PaintingContext context, Offset offset) => Child?.Paint(context, offset);

    public override void Attach(RenderPipeline? owner)
    {
        base.Attach(owner);
        Child?.Attach(owner);
    }

    public override void VisitChildren(Action<RenderObject> visitor)
    {
        if (Child != null) visitor(Child);
    }

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