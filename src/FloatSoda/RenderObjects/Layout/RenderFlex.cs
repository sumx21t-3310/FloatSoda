using FloatSoda.Abstractions.Geometries;
using FloatSoda.Core;
using FloatSoda.Geometrics;
using FloatSoda.Gesture;
using SkiaSharp;

namespace FloatSoda.RenderObjects.Layout;

/// <summary>
/// 複数の子を主軸に沿って並べ、主軸と交差軸の配置を調整するRenderObjectです。
/// </summary>
public class RenderFlex : RenderBox, IHasMultiChildrenRenderObject
{
    /// <summary>
    /// レイアウト対象となる子のコレクションを取得します。
    /// </summary>
    public MultiChildrenCollection<RenderBox> Children { get; }

    /// <summary>
    /// 子を持たないFlexレイアウトを初期化します。
    /// </summary>
    public RenderFlex() => Children = new MultiChildrenCollection<RenderBox>(this);

    /// <summary>
    /// 指定したRenderObjectを末尾の子として追加します。
    /// </summary>
    /// <param name="child">追加するRenderObject。<see cref="RenderBox"/>である必要があります。</param>
    /// <remarks>
    /// このRenderObjectをLayout Dirtyとしてマークし、
    /// 次のパイプライン更新時に全ての子のサイズと位置を再計算します。
    /// </remarks>
    public void AddChild(RenderObject child) => Children.Add((RenderBox)child);

    /// <summary>
    /// 指定したRenderObjectを子のコレクションから削除します。
    /// </summary>
    /// <param name="child">削除するRenderObject。<see cref="RenderBox"/>である必要があります。</param>
    /// <returns>子が見つかり削除された場合は<c>true</c>、それ以外の場合は<c>false</c>。</returns>
    /// <remarks>
    /// 子が削除された場合、このRenderObjectをLayout Dirtyとしてマークし、
    /// 次のパイプライン更新時に残りの子のサイズと位置を再計算します。
    /// 子が見つからなかった場合、Dirty状態は変更されません。
    /// </remarks>
    public bool RemoveChild(RenderObject child) => Children.Remove((RenderBox)child);

    /// <inheritdoc/>
    public override void SetupParentData(RenderObject child) => child.ParentData = new BoxParentData();

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

    /// <summary>
    /// 子を並べる主軸の方向を取得または設定します。
    /// </summary>
    /// <remarks>
    /// 値が変更された場合、このRenderObjectをLayout Dirtyとしてマークし、
    /// 次のパイプライン更新時に各子のサイズと位置を再計算します。
    /// 値が変更されなかった場合、Dirty状態は変更されません。
    /// </remarks>
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

    /// <summary>
    /// 主軸方向の子の配置方法を取得または設定します。
    /// </summary>
    /// <remarks>
    /// 値が変更された場合、このRenderObjectをLayout Dirtyとしてマークし、
    /// 次のパイプライン更新時に主軸方向の余白と各子の位置を再計算します。
    /// 値が変更されなかった場合、Dirty状態は変更されません。
    /// </remarks>
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

    /// <summary>
    /// 主軸方向に利用可能な領域を占有する方法を取得または設定します。
    /// </summary>
    /// <remarks>
    /// 値が変更された場合、このRenderObjectをLayout Dirtyとしてマークし、
    /// 次のパイプライン更新時に主軸方向のサイズと各子の位置を再計算します。
    /// 値が変更されなかった場合、Dirty状態は変更されません。
    /// </remarks>
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

    /// <summary>
    /// 交差軸方向の子の配置方法を取得または設定します。
    /// </summary>
    /// <remarks>
    /// 値が変更された場合、このRenderObjectをLayout Dirtyとしてマークし、
    /// 次のパイプライン更新時に交差軸方向の制約と各子の位置を再計算します。
    /// 値が変更されなかった場合、Dirty状態は変更されません。
    /// </remarks>
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

    /// <summary>
    /// 垂直方向における開始側と終了側の向きを取得または設定します。
    /// </summary>
    /// <remarks>
    /// 値が変更された場合、このRenderObjectをLayout Dirtyとしてマークし、
    /// 次のパイプライン更新時に各子の配置順と位置を再計算します。
    /// 値が変更されなかった場合、Dirty状態は変更されません。
    /// </remarks>
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    internal override SKSize ComputeDryLayout(BoxConstraints constraints)
    {
        double allocatedSize = 0;
        double crossSize = 0;
        foreach (var child in Children)
        {
            var childConstraints = GetChildConstraints(constraints);
            var childSize = child.GetDryLayout(childConstraints);
            allocatedSize += GetMainSize(childSize);
            crossSize = Math.Max(crossSize, GetCrossSize(childSize));
        }

        var maxMainSize = Direction == Axis.Horizontal ? constraints.MaxWidth : constraints.MaxHeight;
        var idealMainSize = double.IsFinite(maxMainSize) && MainAxisSize == MainAxisSize.Max
            ? maxMainSize
            : allocatedSize;
        return Direction switch
        {
            Axis.Horizontal => constraints.Constrain(idealMainSize, crossSize),
            Axis.Vertical => constraints.Constrain(crossSize, idealMainSize),
            _ => throw new ArgumentOutOfRangeException(nameof(Direction), Direction, null)
        };
    }

    /// <inheritdoc/>
    protected override double ComputeMinIntrinsicWidth(double height) => Direction switch
    {
        Axis.Horizontal => Children.Sum(child => child.GetMinIntrinsicWidth(height)),
        Axis.Vertical => Children.Count == 0 ? 0 : Children.Max(child => child.GetMinIntrinsicWidth(height)),
        _ => throw new ArgumentOutOfRangeException(nameof(Direction), Direction, null)
    };

    /// <inheritdoc/>
    protected override double ComputeMaxIntrinsicWidth(double height) => Direction switch
    {
        Axis.Horizontal => Children.Sum(child => child.GetMaxIntrinsicWidth(height)),
        Axis.Vertical => Children.Count == 0 ? 0 : Children.Max(child => child.GetMaxIntrinsicWidth(height)),
        _ => throw new ArgumentOutOfRangeException(nameof(Direction), Direction, null)
    };

    /// <inheritdoc/>
    protected override double ComputeMinIntrinsicHeight(double width) => Direction switch
    {
        Axis.Horizontal => ComputeHorizontalIntrinsicHeight(width, true),
        Axis.Vertical => Children.Sum(child => child.GetMinIntrinsicHeight(width)),
        _ => throw new ArgumentOutOfRangeException(nameof(Direction), Direction, null)
    };

    /// <inheritdoc/>
    protected override double ComputeMaxIntrinsicHeight(double width) => Direction switch
    {
        Axis.Horizontal => ComputeHorizontalIntrinsicHeight(width, false),
        Axis.Vertical => Children.Sum(child => child.GetMaxIntrinsicHeight(width)),
        _ => throw new ArgumentOutOfRangeException(nameof(Direction), Direction, null)
    };

    private BoxConstraints GetChildConstraints(BoxConstraints constraints) => (CrossAxisAlignment, Direction) switch
    {
        (CrossAxisAlignment.Stretch, Axis.Horizontal) when double.IsFinite(constraints.MaxHeight) =>
            BoxConstraints.TightFor(height: constraints.MaxHeight),
        (CrossAxisAlignment.Stretch, Axis.Vertical) when double.IsFinite(constraints.MaxWidth) =>
            BoxConstraints.TightFor(width: constraints.MaxWidth),
        (_, Axis.Horizontal) => new BoxConstraints(MaxHeight: constraints.MaxHeight),
        (_, Axis.Vertical) => new BoxConstraints(MaxWidth: constraints.MaxWidth),
        _ => throw new ArgumentOutOfRangeException(nameof(Direction), Direction, null)
    };

    private double ComputeHorizontalIntrinsicHeight(double width, bool minimum)
    {
        if (Children.Count == 0) return 0;

        var naturalWidths = Children.Select(child => child.GetMaxIntrinsicWidth(double.PositiveInfinity)).ToArray();
        var totalWidth = naturalWidths.Sum();
        var scale = double.IsFinite(width) && totalWidth > width && totalWidth > 0 ? width / totalWidth : 1;
        return Children.Select((child, index) => minimum
                ? child.GetMinIntrinsicHeight(naturalWidths[index] * scale)
                : child.GetMaxIntrinsicHeight(naturalWidths[index] * scale))
            .Max();
    }


    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public override bool HitTestChildren(HitTestResult result, Offset position)
    {
        // 描画順の逆（手前に描かれた子から）に判定し、最初にヒットした子で打ち切る
        foreach (var child in Children.Reverse())
        {
            var childParentData = child.ParentData as BoxParentData;

            var isHit = result.AddWidthPaintOffset(
                childParentData?.Offset,
                position,
                (testResult, transformed) => child.HitTest(testResult, transformed)
            );

            if (isHit) return true;
        }

        return false;
    }
}
