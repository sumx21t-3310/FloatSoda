using FloatSoda.RenderObjects.Gesture;

namespace FloatSoda.Widgets.Gesture;

/// <summary>子要素を描画したままポインター入力を自身で吸収し、子要素へ到達させないウィジェットです。</summary>
/// <seealso cref="RenderAbsorbPointer"/>
public record AbsorbPointer : SingleChildRenderObjectWidget<RenderAbsorbPointer>
{
    /// <summary>ポインターのヒットを吸収するかどうかを取得します。</summary>
    /// <value>既定値は<see langword="true"/>です。</value>
    public bool Absorbing { get; init; } = true;

    /// <inheritdoc />
    public override RenderAbsorbPointer CreateRenderObject() => new() { Absorbing = Absorbing };

    /// <inheritdoc />
    public override void UpdateRenderObject(RenderAbsorbPointer renderObject) => renderObject.Absorbing = Absorbing;
}
