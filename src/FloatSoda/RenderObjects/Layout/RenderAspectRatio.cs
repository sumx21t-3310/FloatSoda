using FloatSoda.Geometrics;
using SkiaSharp;

namespace FloatSoda.RenderObjects.Layout;

/// <summary>指定した幅対高さの比率を満たすように自身と子をレイアウトするRenderObjectです。</summary>
public sealed class RenderAspectRatio : RenderProxyBox
{
    /// <summary>適用する幅対高さの比率を取得または設定します。</summary>
    /// <value>正の有限値。</value>
    /// <exception cref="ArgumentOutOfRangeException">値が0以下または有限値ではありません。</exception>
    public required double AspectRatio
    {
        get;
        set
        {
            ValidateAspectRatio(value, nameof(AspectRatio));
            if (field == value) return;
            field = value;
            MarkNeedsLayout();
        }
    }

    /// <inheritdoc/>
    public override void PerformLayout()
    {
        Size = ApplyAspectRatio(Constraints);
        Child?.Layout(BoxConstraints.Tight(Size));
    }

    /// <inheritdoc/>
    internal override SKSize ComputeDryLayout(BoxConstraints constraints) => ApplyAspectRatio(constraints);

    /// <inheritdoc/>
    protected override double ComputeMinIntrinsicWidth(double height) =>
        double.IsFinite(height) ? height * AspectRatio : Child?.GetMinIntrinsicWidth(height) ?? 0;

    /// <inheritdoc/>
    protected override double ComputeMaxIntrinsicWidth(double height) =>
        double.IsFinite(height) ? height * AspectRatio : Child?.GetMaxIntrinsicWidth(height) ?? 0;

    /// <inheritdoc/>
    protected override double ComputeMinIntrinsicHeight(double width) =>
        double.IsFinite(width) ? width / AspectRatio : Child?.GetMinIntrinsicHeight(width) ?? 0;

    /// <inheritdoc/>
    protected override double ComputeMaxIntrinsicHeight(double width) =>
        double.IsFinite(width) ? width / AspectRatio : Child?.GetMaxIntrinsicHeight(width) ?? 0;

    private SKSize ApplyAspectRatio(BoxConstraints constraints)
    {
        if (constraints.IsTight) return constraints.Smallest;

        if (!double.IsFinite(constraints.MaxWidth) && !double.IsFinite(constraints.MaxHeight))
        {
            throw new InvalidOperationException(
                "RenderAspectRatioは幅と高さの両方が制約されていない場合にサイズを決定できません。");
        }

        var width = constraints.MaxWidth;
        double height;
        if (double.IsFinite(width))
        {
            height = width / AspectRatio;
        }
        else
        {
            height = constraints.MaxHeight;
            width = height * AspectRatio;
        }

        if (width > constraints.MaxWidth)
        {
            width = constraints.MaxWidth;
            height = width / AspectRatio;
        }

        if (height > constraints.MaxHeight)
        {
            height = constraints.MaxHeight;
            width = height * AspectRatio;
        }

        if (width < constraints.MinWidth)
        {
            width = constraints.MinWidth;
            height = width / AspectRatio;
        }

        if (height < constraints.MinHeight)
        {
            height = constraints.MinHeight;
            width = height * AspectRatio;
        }

        return constraints.Constrain(width, height);
    }

    internal static void ValidateAspectRatio(double value, string parameterName)
    {
        if (!double.IsFinite(value) || value <= 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "AspectRatioには正の有限値を指定してください。");
        }
    }
}
