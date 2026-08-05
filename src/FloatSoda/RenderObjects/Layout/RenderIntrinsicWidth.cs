using FloatSoda.Geometrics;
using SkiaSharp;

namespace FloatSoda.RenderObjects.Layout;

/// <summary>子の最大intrinsic幅を使って子の幅を決定するRenderObjectです。</summary>
/// <remarks>
/// intrinsic測定は通常レイアウト前の追加走査を行い、入れ子では最悪O(N²)になり得ます。
/// スクロール領域や大規模なツリーでは固定制約を優先してください。
/// </remarks>
public sealed class RenderIntrinsicWidth : RenderProxyBox
{
    /// <summary>計測した幅を切り上げる単位を取得または設定します。</summary>
    /// <value>正の有限値。丸めを行わない場合は<see langword="null"/>です。</value>
    /// <exception cref="ArgumentOutOfRangeException">値が0以下または非有限値です。</exception>
    public double? StepWidth
    {
        get;
        set
        {
            ValidateStep(value, nameof(StepWidth));
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
    protected override double ComputeMinIntrinsicWidth(double height) =>
        Child is null ? 0 : RoundUp(Child.GetMinIntrinsicWidth(height), StepWidth);

    /// <inheritdoc/>
    protected override double ComputeMaxIntrinsicWidth(double height) =>
        Child is null ? 0 : RoundUp(Child.GetMaxIntrinsicWidth(height), StepWidth);

    private BoxConstraints CreateChildConstraints(BoxConstraints constraints)
    {
        if (constraints.HasTightWidth) return constraints;

        var width = RoundUp(Child!.GetMaxIntrinsicWidth(constraints.MaxHeight), StepWidth);
        if (!double.IsFinite(width))
        {
            throw new InvalidOperationException("IntrinsicWidthで使用する子の最大intrinsic幅は有限値である必要があります。");
        }

        width = constraints.ConstrainWidth(width);
        return new BoxConstraints(width, width, constraints.MinHeight, constraints.MaxHeight);
    }

    internal static double RoundUp(double value, double? step) => step is null ? value : Math.Ceiling(value / step.Value) * step.Value;

    internal static void ValidateStep(double? value, string parameterName)
    {
        if (value is not null && (!double.IsFinite(value.Value) || value.Value <= 0))
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "stepには正の有限値を指定してください。");
        }
    }
}
