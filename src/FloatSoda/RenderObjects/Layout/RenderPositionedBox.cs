using FloatSoda.Common.Geometries;
using FloatSoda.Core;
using FloatSoda.Geometrics;
using static System.Double;

namespace FloatSoda.RenderObjects.Layout;

public class RenderPositionedBox : RenderBox, IHasSingleChildRenderObject
{
    private readonly SingleChildContainer<RenderObject> _child;

    public RenderPositionedBox() => _child = new SingleChildContainer<RenderObject>(this);

    public RenderObject? Child
    {
        get => _child.Child;
        set => _child.Child = value;
    }

    public override void SetupParentData(RenderObject child) => child.ParentData = new BoxParentData();

    public double? WidthFactor
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            MarkNeedsLayout();
        }
    }

    public double? HeightFactor
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            MarkNeedsLayout();
        }
    }


    public Alignment Alignment
    {
        get;
        set
        {
            if (field == value) return;

            field = value;
            MarkNeedsLayout();
        }
    } = default;


    public override void PerformLayout()
    {
        var shrinkWrapWidth = WidthFactor.HasValue || IsPositiveInfinity(Constraints.MaxWidth);
        var shrinkWrapHeight = HeightFactor.HasValue || IsPositiveInfinity(Constraints.MaxHeight);

        if (Child != null)
        {
            Child.ParentData ??= new BoxParentData();
            Child.Layout(Constraints.Loosen, parentUseSize: true);

            Size = Constraints.Constrain(
                width: shrinkWrapWidth ? Child.Size.Width * (WidthFactor ?? 1) : PositiveInfinity,
                height: shrinkWrapHeight ? Child.Size.Height * (HeightFactor ?? 1) : PositiveInfinity
            );

            AlignChild();
        }
        else
        {
            Size = Constraints.Constrain(
                width: shrinkWrapWidth ? 0 : PositiveInfinity,
                height: shrinkWrapHeight ? 0 : PositiveInfinity
            );
        }
    }

    private void AlignChild()
    {
        if (Child?.ParentData is not BoxParentData childParentData) return;
        var offset = Alignment.ComputeOffset(Size, Child.Size);
        childParentData.Offset = offset;
    }

    public override void Paint(PaintingContext context, Offset offset)
    {
        if (Child == null) return;

        var childParentData = Child.ParentData as BoxParentData;
        Child.Paint(context, offset + (childParentData?.Offset ?? Offset.Zero));
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