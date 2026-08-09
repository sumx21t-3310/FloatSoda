using FloatSoda.Geometrics;
using SkiaSharp;

namespace FloatSoda.RenderObjects.Layout;

/// <summary>親制約が無限の軸だけ、指定した最大寸法で子を制限するRenderObjectです。</summary>
public sealed class RenderLimitedBox : RenderProxyBox
{
    /// <summary>幅が制約されていない場合に適用する最大幅を取得または設定します。</summary>
    /// <value>0以上の有限値または正の無限大。</value>
    /// <exception cref="ArgumentOutOfRangeException">値が負、NaN、または負の無限大です。</exception>
    public double MaxWidth
    {
        get;
        set
        {
            ValidateLimit(value, nameof(MaxWidth));
            if (field == value) return;
            field = value;
            MarkNeedsLayout();
        }
    } = double.PositiveInfinity;

    /// <summary>高さが制約されていない場合に適用する最大高さを取得または設定します。</summary>
    /// <value>0以上の有限値または正の無限大。</value>
    /// <exception cref="ArgumentOutOfRangeException">値が負、NaN、または負の無限大です。</exception>
    public double MaxHeight
    {
        get;
        set
        {
            ValidateLimit(value, nameof(MaxHeight));
            if (field == value) return;
            field = value;
            MarkNeedsLayout();
        }
    } = double.PositiveInfinity;

    /// <inheritdoc/>
    public override void PerformLayout()
    {
        var limitedConstraints = LimitConstraints(Constraints);
        if (Child is null)
        {
            Size = limitedConstraints.Smallest;
            return;
        }

        Child.Layout(limitedConstraints, parentUseSize: true);
        Size = Constraints.Constrain(Child.Size);
    }

    /// <inheritdoc/>
    internal override SKSize ComputeDryLayout(BoxConstraints constraints)
    {
        var limitedConstraints = LimitConstraints(constraints);
        var childSize = Child?.GetDryLayout(limitedConstraints) ?? limitedConstraints.Smallest;
        return constraints.Constrain(childSize);
    }

    private BoxConstraints LimitConstraints(BoxConstraints constraints) => new(
        constraints.MinWidth,
        double.IsFinite(constraints.MaxWidth) ? constraints.MaxWidth : constraints.ConstrainWidth(MaxWidth),
        constraints.MinHeight,
        double.IsFinite(constraints.MaxHeight) ? constraints.MaxHeight : constraints.ConstrainHeight(MaxHeight));

    internal static void ValidateLimit(double value, string parameterName)
    {
        if (double.IsNaN(value) || double.IsNegativeInfinity(value) || value < 0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "最大寸法には0以上の有限値または正の無限大を指定してください。");
        }
    }
}
