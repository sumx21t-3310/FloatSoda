using FloatSoda.Geometrics;
using FloatSoda.Rendering.Layers;
using FloatSoda.RenderObjects.Layout;

namespace FloatSoda.Widgets.Layout;

/// <summary>子要素を自身の領域へ拡大縮小し、指定した位置へ配置します。</summary>
/// <seealso cref="RenderFittedBox"/>
public sealed record FittedBox : SingleChildRenderObjectWidget<RenderFittedBox>
{
    /// <summary>子要素を自身の領域へ収める方法を取得します。</summary>
    /// <exception cref="ArgumentOutOfRangeException">定義されていない値です。</exception>
    public BoxFit Fit
    {
        get;
        init
        {
            RenderFittedBox.ValidateFit(value, nameof(Fit));
            field = value;
        }
    } = BoxFit.Contain;

    /// <summary>拡大縮小後の子要素を自身の領域内へ配置する位置を取得します。</summary>
    /// <exception cref="ArgumentOutOfRangeException">いずれかの成分が有限値ではありません。</exception>
    public Alignment Alignment
    {
        get;
        init
        {
            RenderFittedBox.ValidateAlignment(value, nameof(Alignment));
            field = value;
        }
    } = Alignment.Center;

    /// <summary>拡大縮小後の子要素が自身の領域からはみ出す場合の切り抜き方法を取得します。</summary>
    /// <exception cref="ArgumentOutOfRangeException">定義されていない値です。</exception>
    public Clip ClipBehavior
    {
        get;
        init
        {
            RenderFittedBox.ValidateClipBehavior(value, nameof(ClipBehavior));
            field = value;
        }
    } = Clip.None;

    /// <inheritdoc/>
    public override RenderFittedBox CreateRenderObject() => new()
    {
        Fit = Fit,
        Alignment = Alignment,
        ClipBehavior = ClipBehavior,
    };

    /// <inheritdoc/>
    public override void UpdateRenderObject(RenderFittedBox renderObject)
    {
        renderObject.Fit = Fit;
        renderObject.Alignment = Alignment;
        renderObject.ClipBehavior = ClipBehavior;
    }
}
