using FloatSoda.RenderObjects.Layout;

namespace FloatSoda.Widgets.Layout;

/// <summary>子のレイアウト寸法を含め、90度単位で時計回りに回転します。</summary>
/// <remarks><see cref="QuarterTurns"/>には負値や4以上も指定でき、4を法として正規化されます。</remarks>
/// <seealso cref="RenderRotatedBox"/>
public sealed record RotatedBox : SingleChildRenderObjectWidget<RenderRotatedBox>
{
    /// <summary>時計回りに90度回転する回数を取得します。</summary>
    public int QuarterTurns { get; init; }

    /// <inheritdoc/>
    public override RenderRotatedBox CreateRenderObject() => new() { QuarterTurns = QuarterTurns };

    /// <inheritdoc/>
    public override void UpdateRenderObject(RenderRotatedBox renderObject) => renderObject.QuarterTurns = QuarterTurns;
}
