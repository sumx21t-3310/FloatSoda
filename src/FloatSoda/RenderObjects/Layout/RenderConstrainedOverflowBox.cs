using FloatSoda.Abstractions.Geometries;
using FloatSoda.Core;
using FloatSoda.Geometrics;
using FloatSoda.Gesture;
using SkiaSharp;

namespace FloatSoda.RenderObjects.Layout;

/// <summary>親とは異なる制約で子をレイアウトし、自身の領域外への描画を許可するRenderObjectです。</summary>
public sealed class RenderConstrainedOverflowBox : RenderBox, IHasSingleChildRenderObject
{
    private readonly SingleChildContainer<RenderObject> _child;

    /// <summary>子を持たないoverflow用RenderObjectを初期化します。</summary>
    public RenderConstrainedOverflowBox() => _child = new SingleChildContainer<RenderObject>(this);

    /// <summary>配置する子を取得または設定します。</summary>
    public RenderObject? Child
    {
        get => _child.Child;
        set => _child.Child = value;
    }

    /// <summary>子へ渡す最小幅を取得または設定します。</summary>
    /// <value>0以上の有限値。<see langword="null"/>の場合は親の最小幅を使用します。</value>
    public double? MinWidth
    {
        get;
        set
        {
            LayoutOverflowValidation.ValidateMinimum(value, nameof(MinWidth));
            if (field == value) return;
            field = value;
            MarkNeedsLayout();
        }
    }

    /// <summary>子へ渡す最大幅を取得または設定します。</summary>
    /// <value>0以上の値または正の無限大。<see langword="null"/>の場合は親の最大幅を使用します。</value>
    public double? MaxWidth
    {
        get;
        set
        {
            LayoutOverflowValidation.ValidateMaximum(value, nameof(MaxWidth));
            if (field == value) return;
            field = value;
            MarkNeedsLayout();
        }
    }

    /// <summary>子へ渡す最小高さを取得または設定します。</summary>
    /// <value>0以上の有限値。<see langword="null"/>の場合は親の最小高さを使用します。</value>
    public double? MinHeight
    {
        get;
        set
        {
            LayoutOverflowValidation.ValidateMinimum(value, nameof(MinHeight));
            if (field == value) return;
            field = value;
            MarkNeedsLayout();
        }
    }

    /// <summary>子へ渡す最大高さを取得または設定します。</summary>
    /// <value>0以上の値または正の無限大。<see langword="null"/>の場合は親の最大高さを使用します。</value>
    public double? MaxHeight
    {
        get;
        set
        {
            LayoutOverflowValidation.ValidateMaximum(value, nameof(MaxHeight));
            if (field == value) return;
            field = value;
            MarkNeedsLayout();
        }
    }

    /// <summary>自身のサイズを決める方法を取得または設定します。</summary>
    /// <exception cref="ArgumentOutOfRangeException">定義されていない値です。</exception>
    public OverflowBoxFit Fit
    {
        get;
        set
        {
            if (!Enum.IsDefined(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "定義済みのOverflowBoxFitを指定してください。");
            }

            if (field == value) return;
            field = value;
            MarkNeedsLayout();
        }
    } = OverflowBoxFit.Max;

    /// <summary>自身の領域内で子を配置する位置を取得または設定します。</summary>
    /// <exception cref="ArgumentOutOfRangeException">いずれかの成分が非有限値です。</exception>
    public Alignment Alignment
    {
        get;
        set
        {
            LayoutOverflowValidation.ValidateAlignment(value, nameof(Alignment));
            if (field == value) return;
            field = value;
            MarkNeedsLayout();
        }
    } = Alignment.Center;

    /// <inheritdoc/>
    public override void SetupParentData(RenderObject child) => child.ParentData = new BoxParentData();

    /// <inheritdoc/>
    public override void PerformLayout()
    {
        var innerConstraints = GetInnerConstraints(Constraints);
        Child?.Layout(innerConstraints, parentUseSize: true);

        Size = Fit switch
        {
            OverflowBoxFit.Max => GetMaximumSize(Constraints),
            OverflowBoxFit.DeferToChild => Constraints.Constrain(Child?.Size ?? SKSize.Empty),
            _ => throw new ArgumentOutOfRangeException(nameof(Fit), Fit, null)
        };

        if (Child is not null) AlignChild();
    }

    /// <inheritdoc/>
    internal override SKSize ComputeDryLayout(BoxConstraints constraints)
    {
        var innerConstraints = GetInnerConstraints(constraints);
        return Fit switch
        {
            OverflowBoxFit.Max => GetMaximumSize(constraints),
            OverflowBoxFit.DeferToChild => constraints.Constrain(
                Child is null ? SKSize.Empty : GetRenderBoxChild().GetDryLayout(innerConstraints)),
            _ => throw new ArgumentOutOfRangeException(nameof(Fit), Fit, null)
        };
    }

    /// <inheritdoc/>
    protected override double ComputeMinIntrinsicWidth(double height) =>
        Child is null ? 0 : GetRenderBoxChild().GetMinIntrinsicWidth(height);

    /// <inheritdoc/>
    protected override double ComputeMaxIntrinsicWidth(double height) =>
        Child is null ? 0 : GetRenderBoxChild().GetMaxIntrinsicWidth(height);

    /// <inheritdoc/>
    protected override double ComputeMinIntrinsicHeight(double width) =>
        Child is null ? 0 : GetRenderBoxChild().GetMinIntrinsicHeight(width);

    /// <inheritdoc/>
    protected override double ComputeMaxIntrinsicHeight(double width) =>
        Child is null ? 0 : GetRenderBoxChild().GetMaxIntrinsicHeight(width);

    /// <inheritdoc/>
    public override void Paint(PaintingContext context, Offset offset)
    {
        if (Child is null) return;
        var childOffset = ((BoxParentData)Child.ParentData!).Offset;
        context.PaintChild(Child, offset + childOffset);
    }

    /// <inheritdoc/>
    public override bool HitTestChildren(HitTestResult result, Offset position)
    {
        if (Child is not RenderBox child) return false;
        var childOffset = ((BoxParentData)Child.ParentData!).Offset;
        return result.AddWidthPaintOffset(childOffset, position,
            (testResult, transformed) => child.HitTest(testResult, transformed));
    }

    /// <inheritdoc/>
    public override void Attach(RenderPipeline? owner)
    {
        base.Attach(owner);
        _child.Attach(owner);
    }

    /// <inheritdoc/>
    public override void Detach()
    {
        base.Detach();
        _child.Detach();
    }

    /// <inheritdoc/>
    public override void VisitChildren(Action<RenderObject> visitor) => _child.VisitChildren(visitor);

    /// <inheritdoc/>
    public override void RedepthChildren() => VisitChildren(RedepthChild);

    private BoxConstraints GetInnerConstraints(BoxConstraints constraints)
    {
        var minWidth = MinWidth ?? constraints.MinWidth;
        var maxWidth = MaxWidth ?? constraints.MaxWidth;
        var minHeight = MinHeight ?? constraints.MinHeight;
        var maxHeight = MaxHeight ?? constraints.MaxHeight;
        LayoutOverflowValidation.ValidateRange(minWidth, maxWidth, nameof(MinWidth));
        LayoutOverflowValidation.ValidateRange(minHeight, maxHeight, nameof(MinHeight));
        return new BoxConstraints(minWidth, maxWidth, minHeight, maxHeight);
    }

    private static SKSize GetMaximumSize(BoxConstraints constraints)
    {
        if (!double.IsFinite(constraints.MaxWidth) || !double.IsFinite(constraints.MaxHeight))
        {
            throw new InvalidOperationException("OverflowBoxでFit.Maxを使用するには、親の最大幅と最大高さが有限である必要があります。");
        }

        return constraints.Constrain(constraints.MaxWidth, constraints.MaxHeight);
    }

    private RenderBox GetRenderBoxChild() => Child as RenderBox ?? throw new NotSupportedException(
        $"{GetType().Name}のdry layoutにはRenderBoxの子が必要です。");

    private void AlignChild() => ((BoxParentData)Child!.ParentData!).Offset = Alignment.ComputeOffset(Size, Child.Size);
}
