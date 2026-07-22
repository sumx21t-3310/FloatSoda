using FloatSoda.Abstractions.Input;
using FloatSoda.Gesture;

namespace FloatSoda.RenderObjects;

public class RenderPointerListener : RenderProxyBox
{
    public Action<PointerEvent>? OnPointerDown { get; set; }
    public Action<PointerEvent>? OnPointerUp { get; set; }
    public Action<PointerEvent>? OnPointerMove { get; set; }

    public override void HandleEvent(PointerEvent pointerEvent, HitTestEntry entry)
    {
        var action = pointerEvent.Phase switch
        {
            PointerEventPhase.Down => OnPointerDown,
            PointerEventPhase.Move => OnPointerMove,
            PointerEventPhase.Up => OnPointerUp,
            _ => null,
        };

        action?.Invoke(pointerEvent);
    }
}