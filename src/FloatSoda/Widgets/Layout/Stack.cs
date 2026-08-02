using FloatSoda.Geometrics;
using FloatSoda.RenderObjects.Layout;

namespace FloatSoda.Widgets.Layout;

/// <summary>複数の子を同じ領域へ重ねて配置します。</summary>
/// <seealso cref="RenderStack"/>
public sealed record Stack : MultiChildRenderObjectWidget<RenderStack>
{
    /// <summary>非Positioned子と未指定軸のPositioned子を配置する基準位置を取得します。</summary>
    public Alignment Alignment { get; init; } = Alignment.TopLeft;

    /// <summary>非Positioned子へ渡す制約の方式を取得します。</summary>
    public StackFit Fit { get; init; } = StackFit.Loose;

    /// <inheritdoc/>
    public override RenderStack CreateRenderObject() => new()
    {
        Alignment = Alignment,
        Fit = Fit,
    };

    /// <inheritdoc/>
    public override void UpdateRenderObject(RenderStack renderObject)
    {
        renderObject.Alignment = Alignment;
        renderObject.Fit = Fit;
    }
}
