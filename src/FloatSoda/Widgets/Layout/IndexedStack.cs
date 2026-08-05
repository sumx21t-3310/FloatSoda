using FloatSoda.Geometrics;
using FloatSoda.RenderObjects.Layout;

namespace FloatSoda.Widgets.Layout;

/// <summary>すべての子を同じ領域でレイアウトし、指定された一つの子だけを表示します。</summary>
/// <seealso cref="RenderIndexedStack"/>
public sealed record IndexedStack : MultiChildRenderObjectWidget<RenderIndexedStack>
{
    /// <summary>子を配置する基準位置を取得します。</summary>
    public Alignment Alignment { get; init; } = Alignment.TopLeft;

    /// <summary>子へ渡す制約の方式を取得します。</summary>
    public StackFit Fit { get; init; } = StackFit.Loose;

    /// <summary>表示する子の0始まりの位置を取得します。<see langword="null"/>の場合はどの子も表示しません。</summary>
    public int? Index { get; init; } = 0;

    /// <inheritdoc/>
    public override RenderIndexedStack CreateRenderObject()
    {
        ValidateIndex();
        return new RenderIndexedStack
        {
            Alignment = Alignment,
            Fit = Fit,
            Index = Index,
        };
    }

    /// <inheritdoc/>
    public override void UpdateRenderObject(RenderIndexedStack renderObject)
    {
        ValidateIndex();
        renderObject.Alignment = Alignment;
        renderObject.Fit = Fit;
        renderObject.Index = Index;
    }

    private void ValidateIndex()
    {
        if (Index is { } index && (index < 0 || index >= Children.Count))
        {
            throw new ArgumentOutOfRangeException(nameof(Index), Index, "IndexにはChildren内の位置またはnullを指定してください。");
        }
    }
}
