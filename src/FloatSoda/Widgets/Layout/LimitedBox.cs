using FloatSoda.RenderObjects.Layout;

namespace FloatSoda.Widgets.Layout;

/// <summary>親から上限を受け取らなかった軸だけ、子要素へ最大寸法を適用します。</summary>
/// <seealso cref="RenderLimitedBox"/>
public sealed record LimitedBox : SingleChildRenderObjectWidget<RenderLimitedBox>
{
    /// <summary>幅が制約されていない場合に適用する最大幅を取得します。</summary>
    /// <value>0以上の有限値または正の無限大。</value>
    /// <exception cref="ArgumentOutOfRangeException">値が負、NaN、または負の無限大です。</exception>
    public double MaxWidth
    {
        get;
        init
        {
            RenderLimitedBox.ValidateLimit(value, nameof(MaxWidth));
            field = value;
        }
    } = double.PositiveInfinity;

    /// <summary>高さが制約されていない場合に適用する最大高さを取得します。</summary>
    /// <value>0以上の有限値または正の無限大。</value>
    /// <exception cref="ArgumentOutOfRangeException">値が負、NaN、または負の無限大です。</exception>
    public double MaxHeight
    {
        get;
        init
        {
            RenderLimitedBox.ValidateLimit(value, nameof(MaxHeight));
            field = value;
        }
    } = double.PositiveInfinity;

    /// <inheritdoc/>
    public override RenderLimitedBox CreateRenderObject() => new()
    {
        MaxWidth = MaxWidth,
        MaxHeight = MaxHeight,
    };

    /// <inheritdoc/>
    public override void UpdateRenderObject(RenderLimitedBox renderObject)
    {
        renderObject.MaxWidth = MaxWidth;
        renderObject.MaxHeight = MaxHeight;
    }
}
