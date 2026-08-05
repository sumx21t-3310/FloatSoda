using FloatSoda.Geometrics;

namespace FloatSoda.RenderObjects.Layout;

/// <summary>
/// 親から受け取った制約に追加の制約を適用して子をレイアウトするRenderObjectです。
/// </summary>
public class RenderConstrainedBox : RenderProxyBox
{
    /// <summary>
    /// 親の制約へ追加して子に適用する制約を取得または設定します。
    /// </summary>
    /// <remarks>
    /// 値が変更された場合、このRenderObjectをLayout Dirtyとしてマークし、
    /// 次のパイプライン更新時に子のサイズを再計算します。
    /// 値が変更されなかった場合、Dirty状態は変更されません。
    /// </remarks>
    public BoxConstraints AdditionalConstraints
    {
        get;
        set
        {
            if (value == field) return;

            field = value;
            MarkNeedsLayout();
        }
    }

    /// <inheritdoc/>
    public override void PerformLayout()
    {
        var enforcedConstraints = AdditionalConstraints.Enforce(Constraints);
        Child?.Layout(enforcedConstraints, parentUseSize: true);

        Size = Child?.Size ?? enforcedConstraints.Constrain(Size);
    }

    /// <inheritdoc/>
    internal override SkiaSharp.SKSize ComputeDryLayout(BoxConstraints constraints)
    {
        var enforcedConstraints = AdditionalConstraints.Enforce(constraints);
        return Child?.GetDryLayout(enforcedConstraints) ?? enforcedConstraints.Smallest;
    }

    /// <inheritdoc/>
    protected override double ComputeMinIntrinsicWidth(double height) => ComputeIntrinsicWidth(height, true);

    /// <inheritdoc/>
    protected override double ComputeMaxIntrinsicWidth(double height) => ComputeIntrinsicWidth(height, false);

    /// <inheritdoc/>
    protected override double ComputeMinIntrinsicHeight(double width) => ComputeIntrinsicHeight(width, true);

    /// <inheritdoc/>
    protected override double ComputeMaxIntrinsicHeight(double width) => ComputeIntrinsicHeight(width, false);

    private double ComputeIntrinsicWidth(double height, bool minimum)
    {
        if (AdditionalConstraints.HasTightWidth) return AdditionalConstraints.MinWidth;

        var constrainedHeight = AdditionalConstraints.ConstrainHeight(height);
        var childWidth = Child is null
            ? 0
            : minimum ? Child.GetMinIntrinsicWidth(constrainedHeight) : Child.GetMaxIntrinsicWidth(constrainedHeight);
        return AdditionalConstraints.ConstrainWidth(childWidth);
    }

    private double ComputeIntrinsicHeight(double width, bool minimum)
    {
        if (AdditionalConstraints.HasTightHeight) return AdditionalConstraints.MinHeight;

        var constrainedWidth = AdditionalConstraints.ConstrainWidth(width);
        var childHeight = Child is null
            ? 0
            : minimum ? Child.GetMinIntrinsicHeight(constrainedWidth) : Child.GetMaxIntrinsicHeight(constrainedWidth);
        return AdditionalConstraints.ConstrainHeight(childHeight);
    }
}
