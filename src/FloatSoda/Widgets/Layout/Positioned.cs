using FloatSoda.RenderObjects.Layout;

namespace FloatSoda.Widgets.Layout;

/// <summary>Stackの子へ辺からの距離または固定寸法を指定します。</summary>
/// <remarks><see cref="Stack"/>の直接の子として使用します。</remarks>
/// <seealso cref="StackParentData"/>
public sealed record Positioned : ParentDataWidget<StackParentData>
{
    /// <summary>Stack左端から子の左端までの距離を取得します。</summary>
    public double? Left { get; init; }

    /// <summary>Stack上端から子の上端までの距離を取得します。</summary>
    public double? Top { get; init; }

    /// <summary>Stack右端から子の右端までの距離を取得します。</summary>
    public double? Right { get; init; }

    /// <summary>Stack下端から子の下端までの距離を取得します。</summary>
    public double? Bottom { get; init; }

    /// <summary>子へ要求する幅を取得します。</summary>
    public double? Width { get; init; }

    /// <summary>子へ要求する高さを取得します。</summary>
    public double? Height { get; init; }

    /// <inheritdoc/>
    protected override bool ApplyParentData(StackParentData parentData)
    {
        Validate();

        var changed = parentData.Left != Left
            || parentData.Top != Top
            || parentData.Right != Right
            || parentData.Bottom != Bottom
            || parentData.Width != Width
            || parentData.Height != Height;

        if (!changed) return false;

        parentData.Left = Left;
        parentData.Top = Top;
        parentData.Right = Right;
        parentData.Bottom = Bottom;
        parentData.Width = Width;
        parentData.Height = Height;
        return true;
    }

    private void Validate()
    {
        if (CountSpecified(Left, Right, Width) > 2)
        {
            throw new InvalidOperationException("PositionedではLeft、Right、Widthのうち同時に指定できるのは2つまでです。");
        }

        if (CountSpecified(Top, Bottom, Height) > 2)
        {
            throw new InvalidOperationException("PositionedではTop、Bottom、Heightのうち同時に指定できるのは2つまでです。");
        }

        ValidateOffset(Left, nameof(Left));
        ValidateOffset(Top, nameof(Top));
        ValidateOffset(Right, nameof(Right));
        ValidateOffset(Bottom, nameof(Bottom));
        ValidateDimension(Width, nameof(Width));
        ValidateDimension(Height, nameof(Height));
    }

    private static int CountSpecified(double? first, double? second, double? third)
        => (first.HasValue ? 1 : 0) + (second.HasValue ? 1 : 0) + (third.HasValue ? 1 : 0);

    private static void ValidateOffset(double? value, string name)
    {
        if (value.HasValue && !double.IsFinite(value.Value))
        {
            throw new ArgumentOutOfRangeException(name, value, "配置値には有限値を指定してください。");
        }
    }

    private static void ValidateDimension(double? value, string name)
    {
        if (value.HasValue && (!double.IsFinite(value.Value) || value.Value < 0))
        {
            throw new ArgumentOutOfRangeException(name, value, "寸法には0以上の有限値を指定してください。");
        }
    }
}
