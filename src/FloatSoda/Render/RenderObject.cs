using FloatSoda.Common.Geometries;
using FloatSoda.Common.Layer;
using FloatSoda.Geometrics;
using SkiaSharp;

namespace FloatSoda.Render;

public abstract class RenderObject
{
    public IParentData? ParentData { get; set; }

    public abstract SKSize Size { get; protected set; }

    public abstract void Layout(BoxConstraints constraints);
    public abstract void Paint(PaintingContext context, Offset offset);
}

public abstract class RenderBox : RenderObject
{
    public override SKSize Size { get; protected set; } = SKSize.Empty;
}

public abstract class RenderProxyBox(RenderBox? child = null) : RenderBox
{
    public RenderBox? Child { get; set; } = child;

    public override void Layout(BoxConstraints constraints)
    {
        if (Child != null)
        {
            Child.Layout(constraints);
            Size = Child.Size;
        }
        else
        {
            Size = constraints.Smallest;
        }
    }

    public override void Paint(PaintingContext context, Offset offset) => Child?.Paint(context, offset);
}

public class RenderConstrainedBox(BoxConstraints additionalConstraints, RenderBox? child) : RenderProxyBox(child)
{
    public override void Layout(BoxConstraints constraints)
    {
        var enforcedConstraints = additionalConstraints.Enforce(constraints);
        Child?.Layout(enforcedConstraints);

        Size = Child?.Size ?? enforcedConstraints.Constrain(Size);
    }

    public override void Paint(PaintingContext context, Offset offset) => Child?.Paint(context, offset);
}

public class RenderColoredBox : RenderProxyBox
{
    public SKColor Color { get; set; } = SKColors.Black;

    public override void Paint(PaintingContext context, Offset offset)
    {
        if (!Size.IsEmpty)
        {
            context.Canvas.DrawRect(Size.And(offset), new SKPaint { Color = Color });
        }

        Child?.Paint(context, offset);
    }
}

public class RenderView(float width, float height) : RenderObject
{
    public override SKSize Size { get; protected set; } = new(width, height);

    public RenderBox? Child { get; set; }
    public ContainerLayer Layer { get; } = new TransformLayer();


    public void PerformLayout() => Layout(BoxConstraints.Tight(Size));
    public override void Layout(BoxConstraints constraints) => Child?.Layout(constraints);

    public override void Paint(PaintingContext context, Offset offset) => Child?.Paint(context, offset);
}

public class RenderPositionedBox(
    RenderObject? child = null,
    double? widthFactor = null,
    double? heightFactor = null,
    Alignment alignment = default) : RenderBox
{
    public override void Layout(BoxConstraints constraints)
    {
        var shrinkWrapWidth = widthFactor != null || double.IsPositiveInfinity(constraints.MaxWidth);
        var shrinkWrapHeight = heightFactor != null || double.IsPositiveInfinity(constraints.MaxHeight);

        if (child != null)
        {
            child.Layout(constraints.Loosen);
            Size = constraints.Constrain(new SKSize
            {
                Width = shrinkWrapWidth ? (float)(child.Size.Width * (widthFactor ?? 1)) : float.PositiveInfinity,
                Height = shrinkWrapHeight ? (float)(child.Size.Height * (heightFactor ?? 1)) : float.PositiveInfinity
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
        if (child?.ParentData is not BoxParentData childParentData) return;
        childParentData.Offset = alignment.ComputeOffset(Size, child.Size);
    }

    public override void Paint(PaintingContext context, Offset offset)
    {
        if (child == null) return;

        var childParentData = child.ParentData as BoxParentData;
        child.Paint(context, offset + (childParentData?.Offset ?? Offset.Zero));
    }
}

public class RenderFlex : RenderBox
{
    private List<RenderBox> Children { get; set; }
    private Axis Direction { get; }
    private MainAxisAlignment MainAxisAlignment { get; }
    private MainAxisSize MainAxisSize { get; }
    private CrossAxisAlignment CrossAxisAlignment { get; }
    private VerticalDirection VerticalDirection { get; }

    public RenderFlex(List<RenderBox> children,
        Axis direction = Axis.Vertical,
        MainAxisAlignment mainAxisAlignment = MainAxisAlignment.Start,
        MainAxisSize mainAxisSize = MainAxisSize.Max,
        CrossAxisAlignment crossAxisAlignment = CrossAxisAlignment.Center,
        VerticalDirection verticalDirection = VerticalDirection.Down)
    {
        Children = children;
        Direction = direction;
        MainAxisAlignment = mainAxisAlignment;
        MainAxisSize = mainAxisSize;
        CrossAxisAlignment = crossAxisAlignment;
        VerticalDirection = verticalDirection;

        foreach (var child in Children)
        {
            child.ParentData = new BoxParentData();
        }
    }

    public override void Layout(BoxConstraints constraints)
    {
        var maxMainSize = Direction == Axis.Horizontal ? constraints.MaxWidth : constraints.MaxHeight;
        var canFlex = maxMainSize < float.PositiveInfinity;
        double crossSize = 0;
        double allocatedSize = 0;

        foreach (var child in Children)
        {
            var innerConstraints = (CrossAxisAlignment, Direction) switch
            {
                (CrossAxisAlignment.Stretch, Axis.Vertical) => BoxConstraints.TightFor(height: constraints.MaxHeight),
                (CrossAxisAlignment.Stretch, Axis.Horizontal) => BoxConstraints.TightFor(width: constraints.MaxWidth),
                (_, Axis.Vertical) => new BoxConstraints(MaxHeight: constraints.MaxHeight),
                (_, Axis.Horizontal) => new BoxConstraints(MaxWidth: constraints.MaxWidth),
                _ => throw new ArgumentOutOfRangeException()
            };

            child.Layout(innerConstraints);

            var childSize = child.Size;
            allocatedSize += GetMainSize(childSize);
            crossSize = Math.Max(crossSize, GetCrossSize(childSize));
        }

        double idealMainSize = canFlex && MainAxisSize == MainAxisSize.Max ? maxMainSize : allocatedSize;

        if (Direction == Axis.Horizontal)
        {
            Size = constraints.Constrain(new SKSize((float)idealMainSize, (float)crossSize));
            idealMainSize = Size.Width;
            crossSize = Size.Height;
        }
        else
        {
            Size = constraints.Constrain(new SKSize((float)crossSize, (float)idealMainSize));
            idealMainSize = Size.Height;
            crossSize = Size.Width;
        }

        var remainingSpace = Math.Max(0, maxMainSize - idealMainSize);
        var (leadingSpace, betweenSpace) = MainAxisAlignment switch
        {
            MainAxisAlignment.Start => (0, remainingSpace / (Children.Count - 1)),
            MainAxisAlignment.End => (remainingSpace, 0),
            MainAxisAlignment.Center => (remainingSpace / 2, remainingSpace / 2),
            MainAxisAlignment.SpaceBetween => (remainingSpace / (Children.Count - 1), 0),
            MainAxisAlignment.SpaceAround => (remainingSpace / (Children.Count - 1), remainingSpace / (Children.Count - 1)),
            MainAxisAlignment.SpaceEvenly => (0, 0),
            _ => throw new ArgumentOutOfRangeException()
        };
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

    private bool StartIsTopLeft => Direction switch
    {
        Axis.Horizontal => false,
        Axis.Vertical => VerticalDirection == VerticalDirection.Down,
        _ => throw new ArgumentOutOfRangeException()
    };

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
}

public class RenderCustomClip : RenderBox
{
    public override void Layout(BoxConstraints constraints)
    {
        throw new NotImplementedException();
    }

    public override void Paint(PaintingContext context, Offset offset)
    {
        throw new NotImplementedException();
    }
}