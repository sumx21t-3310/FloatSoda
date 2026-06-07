using FloatSoda.Common.Geometries;
using FloatSoda.Geometrics;
using SkiaSharp;

namespace FloatSoda.Render.Layout;

public class RenderPositionedBox : RenderBox
{
    public RenderObject? Child { get; init; } = null;

    public double? WidthFactor { get; init; } = null;
    public double? HeightFactor { get; init; } = null;
    public Alignment Alignment { get; init; } = default;


    public override void Layout(BoxConstraints constraints)
    {
        var shrinkWrapWidth = WidthFactor != null || double.IsPositiveInfinity(constraints.MaxWidth);
        var shrinkWrapHeight = HeightFactor != null || double.IsPositiveInfinity(constraints.MaxHeight);

        if (Child != null)
        {
            Child.ParentData ??= new BoxParentData();
            Child.Layout(constraints.Loosen);
            
            Size = constraints.Constrain(
                width: shrinkWrapWidth ? Child.Size.Width * (WidthFactor ?? 1) : double.PositiveInfinity,
                height: shrinkWrapHeight ? Child.Size.Height * (HeightFactor ?? 1) : double.PositiveInfinity
            );

            AlignChild();
        }
        else
        {
            Size = constraints.Constrain(new SKSize
            {
                Width = shrinkWrapWidth ? 0 : float.PositiveInfinity,
                Height = shrinkWrapHeight ? 0 : float.PositiveInfinity
            });
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