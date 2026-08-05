using FloatSoda.Geometrics;
using SkiaSharp;

namespace FloatSoda.RenderObjects.Layout;

/// <summary>子の最大intrinsic高さを使って子の高さを決定するRenderObjectです。</summary>
/// <remarks>
/// intrinsic測定は通常レイアウト前の追加走査を行い、入れ子では最悪O(N²)になり得ます。
/// スクロール領域や大規模なツリーでは固定制約を優先してください。
/// </remarks>
public sealed class RenderIntrinsicHeight : RenderProxyBox
{
    /// <summary>計測した高さを切り上げる単位を取得または設定します。</summary>
    /// <value>正の有限値。丸めを行わない場合は<see langword="null"/>です。</value>
    /// <exception cref="ArgumentOutOfRangeException">値が0以下または非有限値です。</exception>
    public double? StepHeight
    {
        get;
        set
        {
            RenderIntrinsicWidth.ValidateStep(value, nameof(StepHeight));
            if (field == value) return;
            field = value;
            MarkNeedsLayout();
        }
    }

    /// <inheritdoc/>
    public override void PerformLayout()
    {
        if (Child is null)
        {
            Size = Constraints.Smallest;
            return;
        }

        var childConstraints = CreateChildConstraints(Constraints);
        Child.Layout(childConstraints, parentUseSize: true);
        Size = Child.Size;
    }

    /// <inheritdoc/>
    internal override SKSize ComputeDryLayout(BoxConstraints constraints) =>
        Child?.GetDryLayout(CreateChildConstraints(constraints)) ?? constraints.Smallest;

    /// <inheritdoc/>
    protected override double ComputeMinIntrinsicHeight(double width) =>
        Child is null ? 0 : RenderIntrinsicWidth.RoundUp(Child.GetMinIntrinsicHeight(width), StepHeight);

    /// <inheritdoc/>
    protected override double ComputeMaxIntrinsicHeight(double width) =>
        Child is null ? 0 : RenderIntrinsicWidth.RoundUp(Child.GetMaxIntrinsicHeight(width), StepHeight);

    private BoxConstraints CreateChildConstraints(BoxConstraints constraints)
    {
        if (constraints.HasTightHeight) return constraints;

        var height = RenderIntrinsicWidth.RoundUp(Child!.GetMaxIntrinsicHeight(constraints.MaxWidth), StepHeight);
        if (!double.IsFinite(height))
        {
            throw new InvalidOperationException("IntrinsicHeightで使用する子の最大intrinsic高さは有限値である必要があります。");
        }

        height = constraints.ConstrainHeight(height);
        return new BoxConstraints(constraints.MinWidth, constraints.MaxWidth, height, height);
    }
}
