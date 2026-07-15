using FloatSoda.Abstractions.Geometries;
using FloatSoda.Core;
using FloatSoda.Geometrics;
using SkiaSharp;

namespace FloatSoda.RenderObjects.Layout;

public class RenderFlex : RenderBox, IHasMultiChildrenRenderObject
{
    public MultiChildrenCollection<RenderBox> Children { get; }

    public RenderFlex() => Children = new MultiChildrenCollection<RenderBox>(this);

    public void AddChild(RenderObject child) => Children.Add((RenderBox)child);
    public bool RemoveChild(RenderObject child) => Children.Remove((RenderBox)child);

    public override void SetupParentData(RenderObject child) => child.ParentData = new BoxParentData();

    public override void Attach(RenderPipeline? owner)
    {
        base.Attach(owner);
        Children.Attach(owner);
    }

    public override void Detach()
    {
        base.Detach();
        Children.Detach();
    }

    public override void VisitChildren(Action<RenderObject> visitor) => Children.VisitChildren(visitor);

    public override void RedepthChildren() => VisitChildren(RedepthChild);

    public Axis Direction
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            MarkNeedsLayout();
        }
    } = Axis.Vertical;

    public MainAxisAlignment MainAxisAlignment
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            MarkNeedsLayout();
        }
    } = MainAxisAlignment.Start;

    public MainAxisSize MainAxisSize
    {
        get;
        set
        {
            if (value == field) return;
            field = value;
            MarkNeedsLayout();
        }
    } = MainAxisSize.Max;

    public CrossAxisAlignment CrossAxisAlignment
    {
        get;
        set
        {
            if (value == field) return;
            field = value;
            MarkNeedsLayout();
        }
    } = CrossAxisAlignment.Center;

    public VerticalDirection VerticalDirection
    {
        get;
        set
        {
            if (value == field) return;
            field = value;
            MarkNeedsLayout();
        }
    } = VerticalDirection.Down;

    private float GetMainSize(SKSize size) => Direction switch
    {
        Axis.Horizontal => size.Width,
        Axis.Vertical => size.Height,
        _ => throw new ArgumentOutOfRangeException(nameof(Direction), Direction, null)
    };

    private float GetCrossSize(SKSize size) => Direction switch
    {
        Axis.Horizontal => size.Height,
        Axis.Vertical => size.Width,
        _ => throw new ArgumentOutOfRangeException(nameof(Direction), Direction, null)
    };

    private void MeasureCrossAxis(BoxConstraints constraints, out double allocatedSize, out double crossSize)
    {
        crossSize = 0.0;
        allocatedSize = 0.0;

        foreach (var child in Children)
        {
            child.ParentData ??= new BoxParentData();

            var innerAxisSize = (CrossAxisAlignment, Direction) switch
            {
                (CrossAxisAlignment.Stretch, Axis.Horizontal) => BoxConstraints.TightFor(height: constraints.MaxHeight),
                (CrossAxisAlignment.Stretch, Axis.Vertical) => BoxConstraints.TightFor(width: constraints.MaxWidth),
                (_, Axis.Horizontal) => new BoxConstraints(MaxHeight: constraints.MaxHeight),
                (_, Axis.Vertical) => new BoxConstraints(MaxWidth: constraints.MaxWidth),
            };

            child.Layout(innerAxisSize, parentUseSize: true);
            allocatedSize += GetMainSize(child.Size);
            crossSize = Math.Max(crossSize, GetCrossSize(child.Size));
        }
    }

    private double CalcBetweenSpace(double remainingSpace) => MainAxisAlignment switch
    {
        MainAxisAlignment.Start or MainAxisAlignment.End or MainAxisAlignment.Center => 0.0,
        MainAxisAlignment.SpaceBetween => Children.Count > 1 ? remainingSpace / (Children.Count - 1) : 0.0,
        MainAxisAlignment.SpaceAround => Children.Count > 0 ? remainingSpace / Children.Count : 0.0,
        MainAxisAlignment.SpaceEvenly => Children.Count > 0 ? remainingSpace / (Children.Count + 1) : 0.0,
        _ => 0
    };

    private double CalcLeadingSpace(double remainingSpace, double betweenSpace) => MainAxisAlignment switch
    {
        MainAxisAlignment.Start => 0,
        MainAxisAlignment.End => remainingSpace,
        MainAxisAlignment.Center => remainingSpace / 2.0,
        MainAxisAlignment.SpaceBetween => 0,
        MainAxisAlignment.SpaceAround => betweenSpace / 2.0,
        MainAxisAlignment.SpaceEvenly => betweenSpace,
        _ => 0
    };

    private double CalcChildCrossPosition(double crossSize, RenderBox child) => CrossAxisAlignment switch
    {
        CrossAxisAlignment.Start => Direction.Flip().StartIsTopLeft(VerticalDirection)
            ? 0
            : crossSize - GetCrossSize(child.Size),
        CrossAxisAlignment.End => !Direction.Flip().StartIsTopLeft(VerticalDirection)
            ? 0
            : crossSize - GetCrossSize(child.Size),
        CrossAxisAlignment.Center => crossSize / 2 - GetCrossSize(child.Size) / 2,
        CrossAxisAlignment.Stretch => 0,
        CrossAxisAlignment.Baseline => throw new NotImplementedException(),
        _ => throw new ArgumentOutOfRangeException()
    };

    private void ArrangeChildren(double crossSize, double idealMainSize, double leadingSpace, double betweenSpace)
    {
        var flipMainAxis = !Direction.StartIsTopLeft(VerticalDirection);
        var childMainPosition = flipMainAxis ? idealMainSize - leadingSpace : leadingSpace;

        foreach (var child in Children)
        {
            var childParentData = child.ParentData as BoxParentData;
            var childCrossPosition = CalcChildCrossPosition(crossSize, child);

            if (flipMainAxis)
            {
                childMainPosition -= GetMainSize(child.Size);
            }

            childParentData?.Offset = Direction switch
            {
                Axis.Horizontal => new Offset((float)childMainPosition, (float)childCrossPosition),
                Axis.Vertical => new Offset((float)childCrossPosition, (float)childMainPosition),
                _ => throw new ArgumentOutOfRangeException()
            };

            if (flipMainAxis)
            {
                childMainPosition -= betweenSpace;
            }
            else
            {
                childMainPosition += GetMainSize(child.Size) + betweenSpace;
            }
        }
    }

    public override void PerformLayout()
    {
        MeasureCrossAxis(Constraints, out var allocatedSize, out var crossSize);

        var mainSize = Direction == Axis.Horizontal ? Constraints.MaxWidth : Constraints.MaxHeight;
        var canFlex = mainSize < double.PositiveInfinity;

        var idealMainSize = canFlex && MainAxisSize == MainAxisSize.Max ? mainSize : allocatedSize;

        Size = Direction switch
        {
            Axis.Horizontal => Constraints.Constrain(idealMainSize, crossSize),
            Axis.Vertical => Constraints.Constrain(crossSize, idealMainSize),
            _ => throw new ArgumentOutOfRangeException()
        };

        (idealMainSize, crossSize) = Direction switch
        {
            Axis.Horizontal => (Size.Width, Size.Height),
            Axis.Vertical => (Size.Height, Size.Width),
            _ => throw new ArgumentOutOfRangeException()
        };

        var remainingSpace = Math.Max(0, idealMainSize - allocatedSize);

        var betweenSpace = CalcBetweenSpace(remainingSpace);
        var leadingSpace = CalcLeadingSpace(remainingSpace, betweenSpace);

        ArrangeChildren(crossSize, idealMainSize, leadingSpace, betweenSpace);
    }


    public override void Paint(PaintingContext context, Offset offset)
    {
        foreach (var child in Children)
        {
            if (child.ParentData is BoxParentData childParentData)
            {
                context.PaintChild(child, offset + childParentData.Offset);
            }
        }
    }
}