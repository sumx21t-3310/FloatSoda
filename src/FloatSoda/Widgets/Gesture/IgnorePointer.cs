using FloatSoda.RenderObjects.Gesture;

namespace FloatSoda.Widgets.Gesture;

/// <summary>子要素を描画したまま、自身を含む部分木をポインターのヒットテストから除外するウィジェットです。</summary>
/// <seealso cref="RenderIgnorePointer"/>
public record IgnorePointer : SingleChildRenderObjectWidget<RenderIgnorePointer>
{
    /// <summary>自身を含む部分木へのポインター入力を無視するかどうかを取得します。</summary>
    /// <value>既定値は<see langword="false"/>です。</value>
    public bool Ignoring { get; init; }

    /// <inheritdoc />
    public override RenderIgnorePointer CreateRenderObject() => new() { Ignoring = Ignoring };

    /// <inheritdoc />
    public override void UpdateRenderObject(RenderIgnorePointer renderObject) => renderObject.Ignoring = Ignoring;
}
