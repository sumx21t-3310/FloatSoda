using FloatSoda.Common.Geometries;
using FloatSoda.Core;
using FloatSoda.Geometrics;
using static System.Double;

namespace FloatSoda.Render.Layout;

public class RenderPositionedBox : RenderBox, IHasSingleChildRenderObject<RenderObject>
{
    public RenderObject? Child { get; set; } = null;
    public RenderObject ThisRef => this;

    public double? WidthFactor { get; init; } = null;
    public double? HeightFactor { get; init; } = null;
    public Alignment Alignment { get; init; } = default;


    public override void PerformLayout()
    {
        var shrinkWrapWidth = WidthFactor.HasValue || IsPositiveInfinity(Constraints.MaxWidth);
        var shrinkWrapHeight = HeightFactor.HasValue || IsPositiveInfinity(Constraints.MaxHeight);

        if (Child != null)
        {
            Child.ParentData ??= new BoxParentData();
            Child.Layout(Constraints.Loosen);

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
        Child?.Attach(owner);
    }
}