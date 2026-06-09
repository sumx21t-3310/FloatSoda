using FloatSoda.Common.Geometries;
using FloatSoda.Geometrics;
using FloatSoda.Render.Mixin;
using static System.Double;

namespace FloatSoda.Render.Layout;

public class RenderPositionedBox : RenderBox, IRenderObjectWithChild<RenderObject>
{
    public RenderObject? Child { get; set; } = null;

    public double? WidthFactor { get; init; } = null;
    public double? HeightFactor { get; init; } = null;
    public Alignment Alignment { get; init; } = default;


    public override void Layout(BoxConstraints constraints)
    {
        var shrinkWrapWidth = WidthFactor.HasValue || IsPositiveInfinity(constraints.MaxWidth);
        var shrinkWrapHeight = HeightFactor.HasValue || IsPositiveInfinity(constraints.MaxHeight);

        if (Child != null)
        {
            Child.ParentData ??= new BoxParentData();
            Child.Layout(constraints.Loosen);

            Size = constraints.Constrain(
                width: shrinkWrapWidth ? Child.Size.Width * (WidthFactor ?? 1) : PositiveInfinity,
                height: shrinkWrapHeight ? Child.Size.Height * (HeightFactor ?? 1) : PositiveInfinity
            );

            AlignChild();
        }
        else
        {
            Size = constraints.Constrain(
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
}