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

    /// <summary>
    /// <see cref="Offstage"/>に対応するElementです。Flutterの<c>_OffstageElement</c>に当たります。
    /// </summary>
    /// <seealso cref="RenderOffstage"/>
    private sealed class OffstageElement : SingleChildRenderObjectElement<RenderOffstage>
    {
        /// <summary>
        /// <see cref="IsOffstage"/>が<see langword="false"/>のときだけ、子を列挙します。
        /// </summary>
        /// <param name="visitor">画面に出ている子Elementに一度ずつ適用する処理。</param>
        /// <remarks><see cref="IsOffstage"/>が<see langword="true"/>のあいだ、子は画面に出ていないものとして扱い、何も列挙しません。</remarks>
        internal override void VisitOnstageChildren(Action<Element> visitor)
        {
            if (Widget is Offstage { IsOffstage: false })
                base.VisitOnstageChildren(visitor);
        }
    }
}
