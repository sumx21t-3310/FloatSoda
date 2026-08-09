using FloatSoda.Geometrics;
using FloatSoda.RenderObjects.Layout;

namespace FloatSoda.Widgets.Layout;

/// <summary>指定サイズを自身へ適用し、親から受け取った元の制約で子をレイアウトします。</summary>
/// <seealso cref="RenderSizedOverflowBox"/>
public sealed record SizedOverflowBox : SingleChildRenderObjectWidget<RenderSizedOverflowBox>
{
    /// <summary>自身が採ろうとするサイズを取得します。</summary>
    /// <exception cref="ArgumentOutOfRangeException">幅または高さが負数または非有限値です。</exception>
    public required Size Size
    {
        get;
        init
        {
            RenderSizedOverflowBox.ValidateSize(value, nameof(value));
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
    public override RenderSizedOverflowBox CreateRenderObject() => new()
    {
        RequestedSize = Size,
        Alignment = Alignment
    };

    /// <inheritdoc/>
    public override void UpdateRenderObject(RenderSizedOverflowBox renderObject)
    {
        renderObject.RequestedSize = Size;
        renderObject.Alignment = Alignment;
    }
}
