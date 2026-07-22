using FloatSoda.RenderObjects.Gesture;

namespace FloatSoda.Widgets.Gesture;

public record AbsorbPointer : SingleChildRenderObjectWidget<RenderAbsorbPointer>
{
    public bool Absorbing { get; init; } = true;

    public override RenderAbsorbPointer CreateRenderObject() => new() { Absorbing = Absorbing };

    public override void UpdateRenderObject(RenderAbsorbPointer renderObject) => renderObject.Absorbing = Absorbing;
}
