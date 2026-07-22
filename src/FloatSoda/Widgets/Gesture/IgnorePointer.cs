using FloatSoda.RenderObjects.Gesture;

namespace FloatSoda.Widgets.Gesture;

public record IgnorePointer : SingleChildRenderObjectWidget<RenderIgnorePointer>
{
    public bool Ignoring { get; init; }

    public override RenderIgnorePointer CreateRenderObject() => new() { Ignoring = Ignoring };

    public override void UpdateRenderObject(RenderIgnorePointer renderObject) => renderObject.Ignoring = Ignoring;
}
