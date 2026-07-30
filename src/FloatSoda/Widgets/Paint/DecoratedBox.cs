using FloatSoda.Painting;
using FloatSoda.RenderObjects.Painting;

namespace FloatSoda.Widgets.Paint;

/// <summary>
/// ボックス装飾を子要素の前面または背面へ描画します。
/// </summary>
/// <seealso cref="RenderDecoratedBox"/>
public record DecoratedBox : SingleChildRenderObjectWidget<RenderDecoratedBox>
{
    /// <summary>
    /// 描画するボックス装飾を取得します。
    /// </summary>
    public required BoxDecoration Decoration { get; init; }

    /// <summary>
    /// 装飾を子要素の前面と背面のどちらへ描画するかを取得します。
    /// </summary>
    public DecorationPosition Position { get; init; } = DecorationPosition.Background;

    /// <inheritdoc/>
    public override RenderDecoratedBox CreateRenderObject() => new()
    {
        Decoration = Decoration,
        Position = Position
    };

    /// <inheritdoc/>
    public override void UpdateRenderObject(RenderDecoratedBox renderObject)
    {
        renderObject.Decoration = Decoration;
        renderObject.Position = Position;
    }
}
