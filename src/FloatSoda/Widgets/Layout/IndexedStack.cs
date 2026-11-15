using FloatSoda.Elements;
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
    public override Element CreateElement() => new IndexedStackElement { Widget = this };

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
            throw new ArgumentOutOfRangeException(
                nameof(Index),
                Index,
                "IndexにはChildren内の位置またはnullを指定してください。"
            );
        }
    }

    /// <summary>
    /// <see cref="IndexedStack"/>に対応するElementです。Flutterの<c>_IndexedStackElement</c>に当たります。
    /// </summary>
    /// <seealso cref="RenderIndexedStack"/>
    private sealed class IndexedStackElement : MultiChildRenderObjectElement<RenderIndexedStack>
    {
        /// <summary>
        /// <see cref="Index"/>の位置の子だけを列挙します。
        /// </summary>
        /// <param name="visitor">画面に出ている子Elementに一度だけ適用する処理。</param>
        /// <remarks><see cref="Index"/>が<see langword="null"/>のときは、どの子も列挙しません。</remarks>
        internal override void VisitOnstageChildren(Action<Element> visitor)
        {
            if (Widget is not IndexedStack { Index: { } index })
                return;

            var position = 0;
            VisitChildren(child =>
            {
                if (position++ == index)
                    visitor(child);
            });
        }
    }
}
