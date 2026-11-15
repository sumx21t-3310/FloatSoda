using FloatSoda.Elements;
using FloatSoda.RenderObjects.Layout;

namespace FloatSoda.Widgets.Layout;

/// <summary>子をレイアウトしたまま、描画とヒットテストへの参加を切り替えます。</summary>
/// <seealso cref="RenderOffstage"/>
public sealed record Offstage : SingleChildRenderObjectWidget<RenderOffstage>
{
    /// <summary>子を描画とヒットテストから除外するかを取得します。</summary>
    public bool IsOffstage { get; init; } = true;

    /// <inheritdoc/>
    public override Element CreateElement() => new OffstageElement { Widget = this };

    /// <inheritdoc/>
    public override RenderOffstage CreateRenderObject() => new() { Offstage = IsOffstage };

    /// <inheritdoc/>
    public override void UpdateRenderObject(RenderOffstage renderObject) =>
        renderObject.Offstage = IsOffstage;

    // Flutter の _OffstageElement に当たる。IsOffstage が真のあいだ、子を画面に出ていないものとして扱う。
    private sealed class OffstageElement : SingleChildRenderObjectElement<RenderOffstage>
    {
        internal override void VisitOnstageChildren(Action<Element> visitor)
        {
            if (Widget is Offstage { IsOffstage: false })
                base.VisitOnstageChildren(visitor);
        }
    }
}
