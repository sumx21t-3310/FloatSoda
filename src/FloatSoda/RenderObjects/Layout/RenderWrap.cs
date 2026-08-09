using FloatSoda.Abstractions.Geometries;
using FloatSoda.Core;
using FloatSoda.Geometrics;
using FloatSoda.Gesture;
using SkiaSharp;

namespace FloatSoda.RenderObjects.Layout;

/// <summary>Wrap が子ごとの配置位置を保持する ParentData です。</summary>
public class WrapParentData : BoxParentData
{
}

/// <summary>複数の子を run へ分割し、利用可能な主軸領域で折り返して配置する RenderObject です。</summary>
public class RenderWrap : RenderBox, IHasMultiChildrenRenderObject
{
    private sealed class RunMetrics
    {
        public List<RenderBox> Children { get; } = [];

        public double MainExtent { get; set; }

        public double CrossExtent { get; set; }
    }

    /// <summary>レイアウト対象となる子のコレクションを取得します。</summary>
    public MultiChildrenCollection<RenderBox> Children { get; }

    /// <summary>子を持たない Wrap レイアウトを初期化します。</summary>
    public RenderWrap() => Children = new MultiChildrenCollection<RenderBox>(this);

    /// <summary>子を並べる主軸の方向を取得または設定します。</summary>
    public Axis Direction
    {
        get;
        set
        {
            EnsureDefined(value, nameof(Direction));
            if (field == value) return;
            field = value;
            MarkNeedsLayout();
        }
    } = Axis.Horizontal;

    /// <summary>各 run 内で子を主軸方向へ配置する方法を取得または設定します。</summary>
    public WrapAlignment Alignment
    {
        get;
        set
        {
            EnsureDefined(value, nameof(Alignment));
            if (field == value) return;
            field = value;
            MarkNeedsLayout();
        }
    } = WrapAlignment.Start;

    /// <summary>同じ run にある子同士の最小間隔を取得または設定します。</summary>
    /// <exception cref="ArgumentOutOfRangeException">値が負数または有限値ではありません。</exception>
    public double Spacing
    {
        get;
        set
        {
            EnsureFiniteAndNonNegative(value, nameof(Spacing));
            if (field == value) return;
            field = value;
            MarkNeedsLayout();
        }
    }

    /// <summary>run 全体を交差軸方向へ配置する方法を取得または設定します。</summary>
    public WrapAlignment RunAlignment
    {
        get;
        set
        {
            EnsureDefined(value, nameof(RunAlignment));
            if (field == value) return;
            field = value;
            MarkNeedsLayout();
        }
    } = WrapAlignment.Start;

    /// <summary>隣接する run 同士の最小間隔を取得または設定します。</summary>
    /// <exception cref="ArgumentOutOfRangeException">値が負数または有限値ではありません。</exception>
    public double RunSpacing
    {
        get;
        set
        {
            EnsureFiniteAndNonNegative(value, nameof(RunSpacing));
            if (field == value) return;
            field = value;
            MarkNeedsLayout();
        }
    }

    /// <summary>各 run 内で子を交差軸方向へ配置する方法を取得または設定します。</summary>
    public WrapCrossAlignment CrossAxisAlignment
    {
        get;
        set
        {
            EnsureDefined(value, nameof(CrossAxisAlignment));
            if (field == value) return;
            field = value;
            MarkNeedsLayout();
        }
    } = WrapCrossAlignment.Start;

    /// <summary>垂直方向における開始側と終了側の向きを取得または設定します。</summary>
    public VerticalDirection VerticalDirection
    {
        get;
        set
        {
            EnsureDefined(value, nameof(VerticalDirection));
            if (field == value) return;
            field = value;
            MarkNeedsLayout();
        }
    } = VerticalDirection.Down;

    /// <summary>指定した RenderObject を末尾の子として追加します。</summary>
    /// <param name="child">追加する RenderObject。<see cref="RenderBox"/>である必要があります。</param>
    public void AddChild(RenderObject child) => Children.Add((RenderBox)child);

    /// <summary>指定した RenderObject を子のコレクションから削除します。</summary>
    /// <param name="child">削除する RenderObject。</param>
    /// <returns>子が見つかり削除された場合は <see langword="true"/>。</returns>
    public bool RemoveChild(RenderObject child) => Children.Remove((RenderBox)child);

    /// <inheritdoc/>
    public override void SetupParentData(RenderObject child) => child.ParentData = new WrapParentData();

    /// <inheritdoc/>
    public override void PerformLayout()
    {
        if (Children.Count == 0)
        {
            Size = Constraints.Smallest;
            return;
        }

        var (runs, childrenMainExtent, childrenCrossExtent) = ComputeRuns(
            Constraints,
            (child, constraints) =>
            {
                child.Layout(constraints, parentUseSize: true);
                return child.Size;
            });

        Size = ConstrainAxisSize(Constraints, childrenMainExtent, childrenCrossExtent);
        PositionChildren(runs, GetMainExtent(Size), GetCrossExtent(Size), childrenCrossExtent);
    }

    /// <inheritdoc/>
    internal override SKSize ComputeDryLayout(BoxConstraints constraints)
    {
        if (Children.Count == 0) return constraints.Smallest;

        var (_, mainExtent, crossExtent) = ComputeRuns(
            constraints,
            static (child, childConstraints) => child.GetDryLayout(childConstraints));
        return ConstrainAxisSize(constraints, mainExtent, crossExtent);
    }

    /// <inheritdoc/>
    protected override double ComputeMinIntrinsicWidth(double height) => Direction switch
    {
        Axis.Horizontal => Children.Count == 0
            ? 0
            : Children.Max(child => child.GetMinIntrinsicWidth(double.PositiveInfinity)),
        Axis.Vertical => GetDryLayout(new BoxConstraints(MaxHeight: height)).Width,
        _ => throw new ArgumentOutOfRangeException(nameof(Direction), Direction, null)
    };

    /// <inheritdoc/>
    protected override double ComputeMaxIntrinsicWidth(double height) => Direction switch
    {
        Axis.Horizontal => Children.Sum(child => child.GetMaxIntrinsicWidth(double.PositiveInfinity)),
        Axis.Vertical => GetDryLayout(new BoxConstraints(MaxHeight: height)).Width,
        _ => throw new ArgumentOutOfRangeException(nameof(Direction), Direction, null)
    };

    /// <inheritdoc/>
    protected override double ComputeMinIntrinsicHeight(double width) => Direction switch
    {
        Axis.Horizontal => GetDryLayout(new BoxConstraints(MaxWidth: width)).Height,
        Axis.Vertical => Children.Count == 0
            ? 0
            : Children.Max(child => child.GetMinIntrinsicHeight(double.PositiveInfinity)),
        _ => throw new ArgumentOutOfRangeException(nameof(Direction), Direction, null)
    };

    /// <inheritdoc/>
    protected override double ComputeMaxIntrinsicHeight(double width) => Direction switch
    {
        Axis.Horizontal => GetDryLayout(new BoxConstraints(MaxWidth: width)).Height,
        Axis.Vertical => Children.Sum(child => child.GetMaxIntrinsicHeight(double.PositiveInfinity)),
        _ => throw new ArgumentOutOfRangeException(nameof(Direction), Direction, null)
    };

    private (List<RunMetrics> Runs, double MainExtent, double CrossExtent) ComputeRuns(
        BoxConstraints constraints,
        Func<RenderBox, BoxConstraints, SKSize> layoutChild)
    {
        var childConstraints = Direction switch
        {
            Axis.Horizontal => new BoxConstraints(MaxWidth: constraints.MaxWidth),
            Axis.Vertical => new BoxConstraints(MaxHeight: constraints.MaxHeight),
            _ => throw new ArgumentOutOfRangeException(nameof(Direction), Direction, null)
        };
        var mainAxisLimit = Direction == Axis.Horizontal ? constraints.MaxWidth : constraints.MaxHeight;
        var runs = new List<RunMetrics>();
        RunMetrics? currentRun = null;
        double mainExtent = 0;
        double crossExtent = 0;

        foreach (var child in Children)
        {
            var childSize = layoutChild(child, childConstraints);
            var childMainExtent = GetMainExtent(childSize);
            var childCrossExtent = GetCrossExtent(childSize);
            var needsNewRun = currentRun is not null
                && currentRun.MainExtent + Spacing + childMainExtent - mainAxisLimit > 1e-10;

            if (currentRun is null || needsNewRun)
            {
                if (currentRun is not null)
                {
                    mainExtent = Math.Max(mainExtent, currentRun.MainExtent);
                    crossExtent += currentRun.CrossExtent + RunSpacing;
                }

                currentRun = new RunMetrics();
                runs.Add(currentRun);
            }

            if (currentRun.Children.Count > 0) currentRun.MainExtent += Spacing;
            currentRun.Children.Add(child);
            currentRun.MainExtent += childMainExtent;
            currentRun.CrossExtent = Math.Max(currentRun.CrossExtent, childCrossExtent);
        }

        mainExtent = Math.Max(mainExtent, currentRun!.MainExtent);
        crossExtent += currentRun.CrossExtent;
        return (runs, mainExtent, crossExtent);
    }

    private void PositionChildren(
        List<RunMetrics> runs,
        double containerMainExtent,
        double containerCrossExtent,
        double childrenCrossExtent)
    {
        var flipMainAxis = Direction == Axis.Vertical && VerticalDirection == VerticalDirection.Up;
        var flipCrossAxis = Direction == Axis.Horizontal && VerticalDirection == VerticalDirection.Up;
        var effectiveCrossAlignment = flipCrossAxis ? Flip(CrossAxisAlignment) : CrossAxisAlignment;
        var crossFreeSpace = Math.Max(0, containerCrossExtent - childrenCrossExtent);
        var (runLeadingSpace, runBetweenSpace) = DistributeSpace(
            RunAlignment,
            crossFreeSpace,
            RunSpacing,
            runs.Count,
            flipCrossAxis);
        var crossOffset = runLeadingSpace;
        var orderedRuns = flipCrossAxis ? runs.AsEnumerable().Reverse() : runs;

        foreach (var run in orderedRuns)
        {
            var mainFreeSpace = Math.Max(0, containerMainExtent - run.MainExtent);
            var (childLeadingSpace, childBetweenSpace) = DistributeSpace(
                Alignment,
                mainFreeSpace,
                Spacing,
                run.Children.Count,
                flipMainAxis);
            var mainOffset = childLeadingSpace;
            var orderedChildren = flipMainAxis ? run.Children.AsEnumerable().Reverse() : run.Children;

            foreach (var child in orderedChildren)
            {
                var childMainExtent = GetMainExtent(child.Size);
                var childCrossExtent = GetCrossExtent(child.Size);
                var childCrossOffset = effectiveCrossAlignment switch
                {
                    WrapCrossAlignment.Start => 0,
                    WrapCrossAlignment.End => run.CrossExtent - childCrossExtent,
                    WrapCrossAlignment.Center => (run.CrossExtent - childCrossExtent) / 2,
                    _ => throw new ArgumentOutOfRangeException(nameof(CrossAxisAlignment), CrossAxisAlignment, null)
                };
                ((WrapParentData)child.ParentData!).Offset = ToOffset(mainOffset, crossOffset + childCrossOffset);
                mainOffset += childMainExtent + childBetweenSpace;
            }

            crossOffset += run.CrossExtent + runBetweenSpace;
        }
    }

    private static (double LeadingSpace, double BetweenSpace) DistributeSpace(
        WrapAlignment alignment,
        double freeSpace,
        double itemSpacing,
        int itemCount,
        bool flipped) => alignment switch
    {
        WrapAlignment.Start => (flipped ? freeSpace : 0, itemSpacing),
        WrapAlignment.End => (flipped ? 0 : freeSpace, itemSpacing),
        WrapAlignment.Center => (freeSpace / 2, itemSpacing),
        WrapAlignment.SpaceBetween when itemCount < 2 => (flipped ? freeSpace : 0, itemSpacing),
        WrapAlignment.SpaceBetween => (0, freeSpace / (itemCount - 1) + itemSpacing),
        WrapAlignment.SpaceAround => (freeSpace / itemCount / 2, freeSpace / itemCount + itemSpacing),
        WrapAlignment.SpaceEvenly => (freeSpace / (itemCount + 1), freeSpace / (itemCount + 1) + itemSpacing),
        _ => throw new ArgumentOutOfRangeException(nameof(alignment), alignment, null)
    };

    private double GetMainExtent(SKSize size) => Direction == Axis.Horizontal ? size.Width : size.Height;

    private double GetCrossExtent(SKSize size) => Direction == Axis.Horizontal ? size.Height : size.Width;

    private Offset ToOffset(double mainOffset, double crossOffset) => Direction switch
    {
        Axis.Horizontal => new Offset(mainOffset, crossOffset),
        Axis.Vertical => new Offset(crossOffset, mainOffset),
        _ => throw new ArgumentOutOfRangeException(nameof(Direction), Direction, null)
    };

    private SKSize ConstrainAxisSize(BoxConstraints constraints, double mainExtent, double crossExtent) => Direction switch
    {
        Axis.Horizontal => constraints.Constrain(mainExtent, crossExtent),
        Axis.Vertical => constraints.Constrain(crossExtent, mainExtent),
        _ => throw new ArgumentOutOfRangeException(nameof(Direction), Direction, null)
    };

    private static WrapCrossAlignment Flip(WrapCrossAlignment alignment) => alignment switch
    {
        WrapCrossAlignment.Start => WrapCrossAlignment.End,
        WrapCrossAlignment.End => WrapCrossAlignment.Start,
        WrapCrossAlignment.Center => WrapCrossAlignment.Center,
        _ => throw new ArgumentOutOfRangeException(nameof(alignment), alignment, null)
    };

    private static void EnsureFiniteAndNonNegative(double value, string parameterName)
    {
        if (!double.IsFinite(value) || value < 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "間隔には0以上の有限値を指定してください。");
        }
    }

    private static void EnsureDefined<T>(T value, string parameterName) where T : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "定義済みの値を指定してください。");
        }
    }

    /// <inheritdoc/>
    public override void Paint(PaintingContext context, Offset offset)
    {
        foreach (var child in Children)
        {
            context.PaintChild(child, offset + ((WrapParentData)child.ParentData!).Offset);
        }
    }

    /// <inheritdoc/>
    public override bool HitTestChildren(HitTestResult result, Offset position)
    {
        foreach (var child in Children.Reverse())
        {
            var parentData = (WrapParentData)child.ParentData!;
            if (result.AddWidthPaintOffset(
                    parentData.Offset,
                    position,
                    (testResult, transformed) => child.HitTest(testResult, transformed)))
            {
                return true;
            }
        }

        return false;
    }

    /// <inheritdoc/>
    public override void Attach(RenderPipeline? owner)
    {
        base.Attach(owner);
        Children.Attach(owner);
    }

    /// <inheritdoc/>
    public override void Detach()
    {
        base.Detach();
        Children.Detach();
    }

    /// <inheritdoc/>
    public override void VisitChildren(Action<RenderObject> visitor) => Children.VisitChildren(visitor);

    /// <inheritdoc/>
    public override void RedepthChildren() => VisitChildren(RedepthChild);
}
