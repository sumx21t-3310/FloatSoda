using FloatSoda.Geometrics;
using FloatSoda.RenderObjects.Layout;

namespace FloatSoda.Widgets.Layout;

/// <summary>親の最大サイズに対する割合で子をサイズし、自身の領域外への描画を許可します。</summary>
/// <seealso cref="RenderFractionallySizedOverflowBox"/>
public sealed record FractionallySizedBox : SingleChildRenderObjectWidget<RenderFractionallySizedOverflowBox>
{
    /// <summary>親の最大幅へ乗算して子へ適用する幅を取得します。</summary>
    /// <value>0以上の有限値。<see langword="null"/>の場合は親の幅制約をそのまま渡します。</value>
    /// <exception cref="ArgumentOutOfRangeException">値が負数または非有限値です。</exception>
    public double? WidthFactor
    {
        get;
        init
        {
            RenderFractionallySizedOverflowBox.ValidateFactor(value, nameof(value));
            field = value;
        }
    }

    /// <summary>親の最大高さへ乗算して子へ適用する高さを取得します。</summary>
    /// <value>0以上の有限値。<see langword="null"/>の場合は親の高さ制約をそのまま渡します。</value>
    /// <exception cref="ArgumentOutOfRangeException">値が負数または非有限値です。</exception>
    public double? HeightFactor
    {
        get;
        init
        {
            RenderFractionallySizedOverflowBox.ValidateFactor(value, nameof(value));
            field = value;
        }
    }

    /// <summary>自身の領域内で子を配置する位置を取得します。</summary>
    /// <exception cref="ArgumentOutOfRangeException">いずれかの成分が非有限値です。</exception>
    public Alignment Alignment
    {
        get;
        init
        {
            LayoutOverflowValidation.ValidateAlignment(value, nameof(value));
            field = value;
        }
    } = Alignment.Center;

    /// <inheritdoc/>
    public override RenderFractionallySizedOverflowBox CreateRenderObject() => new()
    {
        WidthFactor = WidthFactor,
        HeightFactor = HeightFactor,
        Alignment = Alignment
    };

    /// <inheritdoc/>
    public override void UpdateRenderObject(RenderFractionallySizedOverflowBox renderObject)
    {
        renderObject.WidthFactor = WidthFactor;
        renderObject.HeightFactor = HeightFactor;
        renderObject.Alignment = Alignment;
    }
}
