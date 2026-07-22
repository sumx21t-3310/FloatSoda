using FloatSoda.Abstractions.Input;
using FloatSoda.RenderObjects;

namespace FloatSoda.Widgets.Gesture;

public record Listener : SingleChildRenderObjectWidget<RenderPointerListener>
{
    public Action<PointerEvent>? OnPointerDown { get; init; }
    public Action<PointerEvent>? OnPointerUp { get; init; }
    public Action<PointerEvent>? OnPointerMove { get; init; }

    public override RenderPointerListener CreateRenderObject()
    {
        return new RenderPointerListener()
        {
            OnPointerDown = OnPointerDown,
            OnPointerUp = OnPointerUp,
            OnPointerMove = OnPointerMove,
        };
    }

    public override void UpdateRenderObject(RenderPointerListener renderObject)
    {
        renderObject.OnPointerDown = OnPointerDown;
        renderObject.OnPointerUp = OnPointerUp;
        renderObject.OnPointerMove = OnPointerMove;
    }
}
