using FloatSoda.RenderObjects.Painting;

namespace FloatSoda.Widgets.Paint;

/// <summary>
/// 子サブツリーの再描画を独立させ、変更されていない祖先の再描画を防ぐウィジェットです。
/// </summary>
/// <seealso cref="RenderRepaintBoundary"/>
public record RepaintBoundary : SingleChildRenderObjectWidget<RenderRepaintBoundary>
{
    /// <inheritdoc/>
    public override RenderRepaintBoundary CreateRenderObject() => new();
}
