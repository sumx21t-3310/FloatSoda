using FloatSoda.Geometrics;
using FloatSoda.RenderObjects.Layout;

namespace FloatSoda.Widgets.Layout;

/// <summary>親とは異なる制約で子をレイアウトし、自身の領域外への描画を許可します。</summary>
/// <seealso cref="RenderConstrainedOverflowBox"/>
public sealed record OverflowBox : SingleChildRenderObjectWidget<RenderConstrainedOverflowBox>
{
    /// <summary>子へ渡す最小幅を取得します。</summary>
    /// <value>0以上の有限値。<see langword="null"/>の場合は親の最小幅を使用します。</value>
    public double? MinWidth
    {
        get;
        init
        {
            LayoutOverflowValidation.ValidateMinimum(value, nameof(value));
            if (value is { } minimum && MaxWidth is { } maximum)
            {
                LayoutOverflowValidation.ValidateRange(minimum, maximum, nameof(value));
            }
            field = value;
        }
    }

    /// <summary>子へ渡す最大幅を取得します。</summary>
    /// <value>0以上の値または正の無限大。<see langword="null"/>の場合は親の最大幅を使用します。</value>
    public double? MaxWidth
    {
        get;
        init
        {
            LayoutOverflowValidation.ValidateMaximum(value, nameof(value));
            if (MinWidth is { } minimum && value is { } maximum)
            {
                LayoutOverflowValidation.ValidateRange(minimum, maximum, nameof(value));
            }
            field = value;
        }
    }

    /// <summary>子へ渡す最小高さを取得します。</summary>
    /// <value>0以上の有限値。<see langword="null"/>の場合は親の最小高さを使用します。</value>
    public double? MinHeight
    {
        get;
        init
        {
            LayoutOverflowValidation.ValidateMinimum(value, nameof(value));
            if (value is { } minimum && MaxHeight is { } maximum)
            {
                LayoutOverflowValidation.ValidateRange(minimum, maximum, nameof(value));
            }
            field = value;
        }
    }

    /// <summary>子へ渡す最大高さを取得します。</summary>
    /// <value>0以上の値または正の無限大。<see langword="null"/>の場合は親の最大高さを使用します。</value>
    public double? MaxHeight
    {
        get;
        init
        {
            LayoutOverflowValidation.ValidateMaximum(value, nameof(value));
            if (MinHeight is { } minimum && value is { } maximum)
            {
                LayoutOverflowValidation.ValidateRange(minimum, maximum, nameof(value));
            }
            field = value;
        }
    }

    /// <summary>自身のサイズを決める方法を取得します。</summary>
    /// <exception cref="ArgumentOutOfRangeException">定義されていない値です。</exception>
    public OverflowBoxFit Fit
    {
        get;
        init
        {
            if (!Enum.IsDefined(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "定義済みのOverflowBoxFitを指定してください。");
            }

            field = value;
        }
    } = OverflowBoxFit.Max;

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
    public override RenderConstrainedOverflowBox CreateRenderObject()
    {
        ValidateRanges();
        return new RenderConstrainedOverflowBox
        {
            MinWidth = MinWidth,
            MaxWidth = MaxWidth,
            MinHeight = MinHeight,
            MaxHeight = MaxHeight,
            Fit = Fit,
            Alignment = Alignment
        };
    }

    /// <inheritdoc/>
    public override void UpdateRenderObject(RenderConstrainedOverflowBox renderObject)
    {
        ValidateRanges();
        renderObject.MinWidth = MinWidth;
        renderObject.MaxWidth = MaxWidth;
        renderObject.MinHeight = MinHeight;
        renderObject.MaxHeight = MaxHeight;
        renderObject.Fit = Fit;
        renderObject.Alignment = Alignment;
    }

    private void ValidateRanges()
    {
        if (MinWidth is { } minWidth && MaxWidth is { } maxWidth)
        {
            LayoutOverflowValidation.ValidateRange(minWidth, maxWidth, nameof(MinWidth));
        }

        if (MinHeight is { } minHeight && MaxHeight is { } maxHeight)
        {
            LayoutOverflowValidation.ValidateRange(minHeight, maxHeight, nameof(MinHeight));
        }
    }
}
