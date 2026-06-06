using FloatSoda.Common.Geometries;
using FloatSoda.Geometrics;
using SkiaSharp;

namespace FloatSoda.Render;

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
            Size = constraints.Constrain(new SKSize
            {
                Width = shrinkWrapWidth ? (float)(Child.Size.Width * (WidthFactor ?? 1)) : float.PositiveInfinity,
                Height = shrinkWrapHeight ? (float)(Child.Size.Height * (HeightFactor ?? 1)) : float.PositiveInfinity
            });

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

public class RenderConstrainedBox : RenderProxyBox
{
    public required BoxConstraints AdditionalConstraints { get; init; }

    public RenderConstrainedBox() => Child?.ParentData = new BoxParentData();

    public override void Layout(BoxConstraints constraints)
    {
        var enforcedConstraints = AdditionalConstraints.Enforce(constraints);
        Child?.Layout(enforcedConstraints);

        Size = Child?.Size ?? enforcedConstraints.Constrain(Size);
    }

    public override void Paint(PaintingContext context, Offset offset) => Child?.Paint(context, offset);
}

public class RenderFlex : RenderBox
{
    public List<RenderBox> Children
    {
        get;
        init
        {
            field = value;
            value.ForEach(child => child.ParentData = new BoxParentData());
        }
    } = [];

    public Axis Direction { get; init; } = Axis.Vertical;
    public MainAxisAlignment MainAxisAlignment { get; init; } = MainAxisAlignment.Start;
    public MainAxisSize MainAxisSize { get; init; } = MainAxisSize.Max;
    public CrossAxisAlignment CrossAxisAlignment { get; init; } = CrossAxisAlignment.Center;
    public VerticalDirection VerticalDirection { get; init; } = VerticalDirection.Down;

    public override void Layout(BoxConstraints constraints)
    {
        var mainSize = Direction == Axis.Horizontal ? constraints.MaxWidth : constraints.MaxHeight;
        var canFlex = mainSize < double.PositiveInfinity;
        var crossSize = 0.0;
        var allocatedSize = 0.0;

        foreach (var child in Children)
        {
            var innerAxisSize = (CrossAxisAlignment, Direction) switch
            {
                (CrossAxisAlignment.Stretch, Axis.Horizontal) => BoxConstraints.TightFor(height: constraints.MaxHeight),
                (CrossAxisAlignment.Stretch, Axis.Vertical) => BoxConstraints.TightFor(width: constraints.MaxWidth),
                (_, Axis.Horizontal) => new BoxConstraints(MaxHeight: constraints.MaxHeight),
                (_, Axis.Vertical) => new BoxConstraints(MaxWidth: constraints.MaxWidth),
            };

            child.Layout(innerAxisSize);
            allocatedSize += GetMainSize(child.Size);
            crossSize = Math.Max(crossSize, GetCrossSize(child.Size));
        }

        var idealMainSize = canFlex && MainAxisSize == MainAxisSize.Max ? mainSize : allocatedSize;

        Size = Direction switch
        {
            Axis.Horizontal =>
                constraints.Constrain(new SKSize((float)idealMainSize, (float)crossSize)),
            Axis.Vertical =>
                constraints.Constrain(new SKSize((float)crossSize, (float)idealMainSize)),
            _ => throw new ArgumentOutOfRangeException()
        };

        (idealMainSize, crossSize) = Direction switch
        {
            Axis.Horizontal => (Size.Width, Size.Height),
            Axis.Vertical => (Size.Height, Size.Width),
            _ => throw new ArgumentOutOfRangeException()
        };

        var remainingSpace = Math.Max(0, mainSize - idealMainSize);

        double betweenSpace = MainAxisAlignment switch
        {
            MainAxisAlignment.Start => 0,
            MainAxisAlignment.End => 0,
            MainAxisAlignment.Center => 0,
            MainAxisAlignment.SpaceBetween => Children.Count > 1 ? remainingSpace / (Children.Count - 1) : 0,
            MainAxisAlignment.SpaceAround => Children.Count > 0 ? remainingSpace / Children.Count : 0,
            MainAxisAlignment.SpaceEvenly => Children.Count > 0 ? remainingSpace / (Children.Count + 1) : 0,
            _ => 0
        };


        double leadingSpace = MainAxisAlignment switch
        {
            MainAxisAlignment.Start => 0,
            MainAxisAlignment.End => remainingSpace,
            MainAxisAlignment.Center => remainingSpace / 2,
            MainAxisAlignment.SpaceBetween => 0,
            MainAxisAlignment.SpaceAround => betweenSpace / 2,
            MainAxisAlignment.SpaceEvenly => betweenSpace,
            _ => 0
        };


        var flipMainAxis = !StartIsTopLeft(Direction, VerticalDirection);
        var childMainPosition = flipMainAxis ? idealMainSize - leadingSpace : leadingSpace;

        foreach (var child in Children)
        {
            var childParentData = child.ParentData as BoxParentData;
            double childCrossPosition = CrossAxisAlignment switch
            {
                CrossAxisAlignment.Start => StartIsTopLeft(Direction.Flip(), VerticalDirection)
                    ? 0
                    : crossSize - GetCrossSize(child.Size),
                CrossAxisAlignment.End => StartIsTopLeft(Direction.Flip(), VerticalDirection)
                    ? crossSize - GetCrossSize(child.Size)
                    : 0,
                CrossAxisAlignment.Center => crossSize / 2 - GetCrossSize(child.Size) / 2,
                CrossAxisAlignment.Stretch => 0,
                CrossAxisAlignment.Baseline => throw new NotImplementedException(),
                _ => throw new ArgumentOutOfRangeException()
            };

            if (flipMainAxis)
            {
                childMainPosition -= GetMainSize(child.Size);
            }

            childParentData?.Offset = Direction switch
            {
                Axis.Horizontal => new Offset((float)childMainPosition, (float)childCrossPosition),
                Axis.Vertical => new Offset((float)childCrossPosition, (float)childMainPosition),
            };

            childMainPosition += flipMainAxis ? -betweenSpace : GetMainSize(child.Size) + betweenSpace;
        }
    }

    public override void Paint(PaintingContext context, Offset offset)
    {
        foreach (var child in Children)
        {
            if (child.ParentData is BoxParentData childParentData)
            {
                child.Paint(context, offset + childParentData.Offset);
            }
        }
    }

    private float GetMainSize(SKSize size) => Direction switch
    {
        Axis.Horizontal => size.Width,
        Axis.Vertical => size.Height,
        _ => throw new ArgumentOutOfRangeException()
    };

    private float GetCrossSize(SKSize size) => Direction switch
    {
        Axis.Horizontal => size.Height,
        Axis.Vertical => size.Width,
        _ => throw new ArgumentOutOfRangeException()
    };

    private bool StartIsTopLeft(Axis direction, VerticalDirection verticalDirection)
    {
        return (direction, verticalDirection) switch
        {
            (Axis.Horizontal, _) => true,
            (_, VerticalDirection.Down) => true,
            (_, VerticalDirection.Up) => false,
        };
    }
}