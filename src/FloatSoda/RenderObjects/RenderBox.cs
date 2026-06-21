using FloatSoda.Common.Geometries;
using FloatSoda.Core;
using SkiaSharp;

namespace FloatSoda.RenderObjects;

public abstract class RenderBox : RenderObject
{
    public override SKSize Size { get; protected set; } = SKSize.Empty;
}

public abstract class RenderProxyBox : RenderBox, IHasSingleChildRenderObject<RenderBox>
{
    public RenderBox? Child { get; set; }

    public RenderObject ThisRef => this;

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
        Child?.Attach(owner);
    }

    public override void VisitChildren(Action<RenderObject> visitor)
    {
        if (Child != null) visitor(Child);
    }
}