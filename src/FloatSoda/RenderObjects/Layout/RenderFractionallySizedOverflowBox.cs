using FloatSoda.Abstractions.Geometries;
using FloatSoda.Core;
using FloatSoda.Geometrics;
using FloatSoda.Gesture;
using SkiaSharp;

namespace FloatSoda.RenderObjects.Layout;

/// <summary>親の最大サイズに対する割合で子をレイアウトし、自身の領域外への描画を許可するRenderObjectです。</summary>
public sealed class RenderFractionallySizedOverflowBox : RenderBox, IHasSingleChildRenderObject
{
    private readonly SingleChildContainer<RenderObject> _child;

    /// <summary>子を持たない割合指定RenderObjectを初期化します。</summary>
    public RenderFractionallySizedOverflowBox() => _child = new SingleChildContainer<RenderObject>(this);

    /// <summary>配置する子を取得または設定します。</summary>
    public RenderObject? Child
    {
        get => _child.Child;
        set => _child.Child = value;
    }

    /// <summary>親の最大幅へ乗算して子へ適用する幅を取得または設定します。</summary>
    /// <value>0以上の有限値。<see langword="null"/>の場合は親の幅制約をそのまま渡します。</value>
    /// <exception cref="ArgumentOutOfRangeException">値が負数または非有限値です。</exception>
    public double? WidthFactor
    {
        get;
        set
        {
            ValidateFactor(value, nameof(WidthFactor));
            if (field == value) return;
            field = value;
            MarkNeedsLayout();
        }
    }

    /// <summary>親の最大高さへ乗算して子へ適用する高さを取得または設定します。</summary>
    /// <value>0以上の有限値。<see langword="null"/>の場合は親の高さ制約をそのまま渡します。</value>
    /// <exception cref="ArgumentOutOfRangeException">値が負数または非有限値です。</exception>
    public double? HeightFactor
    {
        get;
        set
        {
            ValidateFactor(value, nameof(HeightFactor));
            if (field == value) return;
            field = value;
            MarkNeedsLayout();
        }
    }

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
        if (Child is null)
        {
            Size = Constraints.Constrain(innerConstraints.Constrain(0, 0));
            return;
        }

        Child.Layout(innerConstraints, parentUseSize: true);
        Size = Constraints.Constrain(Child.Size);
        AlignChild();
    }

    /// <inheritdoc/>
    internal override SKSize ComputeDryLayout(BoxConstraints constraints)
    {
        var innerConstraints = GetInnerConstraints(constraints);
        if (Child is null) return constraints.Constrain(innerConstraints.Constrain(0, 0));
        return constraints.Constrain(GetRenderBoxChild().GetDryLayout(innerConstraints));
    }

    /// <inheritdoc/>
    protected override double ComputeMinIntrinsicWidth(double height) => ComputeIntrinsicWidth(height, true);

    /// <inheritdoc/>
    protected override double ComputeMaxIntrinsicWidth(double height) => ComputeIntrinsicWidth(height, false);

    /// <inheritdoc/>
    protected override double ComputeMinIntrinsicHeight(double width) => ComputeIntrinsicHeight(width, true);

    /// <inheritdoc/>
    protected override double ComputeMaxIntrinsicHeight(double width) => ComputeIntrinsicHeight(width, false);

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
        var width = WidthFactor is null
            ? (Min: constraints.MinWidth, Max: constraints.MaxWidth)
            : GetFractionalAxis(constraints.MaxWidth, WidthFactor.Value, "幅");
        var height = HeightFactor is null
            ? (Min: constraints.MinHeight, Max: constraints.MaxHeight)
            : GetFractionalAxis(constraints.MaxHeight, HeightFactor.Value, "高さ");
        return new BoxConstraints(width.Min, width.Max, height.Min, height.Max);
    }

    private static (double Min, double Max) GetFractionalAxis(double maximum, double factor, string axisName)
    {
        if (!double.IsFinite(maximum))
        {
            throw new InvalidOperationException($"FractionallySizedBoxで{axisName}のfactorを使用するには、親の最大{axisName}が有限である必要があります。");
        }

        var extent = maximum * factor;
        if (!double.IsFinite(extent))
        {
            throw new InvalidOperationException($"FractionallySizedBoxで計算した{axisName}が有限値の範囲を超えました。");
        }

        return (extent, extent);
    }

    private double ComputeIntrinsicWidth(double height, bool minimum)
    {
        if (Child is null) return 0;
        if (WidthFactor == 0) return 0;
        var adjustedHeight = HeightFactor == 0 ? 0 : height * (HeightFactor ?? 1);
        var result = minimum
            ? GetRenderBoxChild().GetMinIntrinsicWidth(adjustedHeight)
            : GetRenderBoxChild().GetMaxIntrinsicWidth(adjustedHeight);
        return result / (WidthFactor ?? 1);
    }

    private double ComputeIntrinsicHeight(double width, bool minimum)
    {
        if (Child is null) return 0;
        if (HeightFactor == 0) return 0;
        var adjustedWidth = WidthFactor == 0 ? 0 : width * (WidthFactor ?? 1);
        var result = minimum
            ? GetRenderBoxChild().GetMinIntrinsicHeight(adjustedWidth)
            : GetRenderBoxChild().GetMaxIntrinsicHeight(adjustedWidth);
        return result / (HeightFactor ?? 1);
    }

    private RenderBox GetRenderBoxChild() => Child as RenderBox ?? throw new NotSupportedException(
        $"{GetType().Name}のintrinsic測定にはRenderBoxの子が必要です。");

    private void AlignChild() => ((BoxParentData)Child!.ParentData!).Offset = Alignment.ComputeOffset(Size, Child.Size);

    internal static void ValidateFactor(double? value, string parameterName)
    {
        if (value is not null && (!double.IsFinite(value.Value) || value.Value < 0))
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "factorには0以上の有限値を指定してください。");
        }
    }
}
