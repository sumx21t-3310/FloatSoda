using FloatSoda.Abstractions.Input;
using FloatSoda.Gesture;
using FloatSoda.RenderObjects.Gesture;

namespace FloatSoda.Widgets.Gesture;

public record Listener : SingleChildRenderObjectWidget<RenderPointerListener>
{
    public Action<PointerEvent>? OnPointerDown { get; init; }
    public Action<PointerEvent>? OnPointerUp { get; init; }
    public Action<PointerEvent>? OnPointerMove { get; init; }

    /// <summary>ヒットテストでの振る舞い。既定は子がヒットした時だけ反応する <see cref="HitTestBehaviour.DeferToChild"/>。</summary>
    public HitTestBehaviour Behaviour { get; init; } = HitTestBehaviour.DeferToChild;

    public override RenderPointerListener CreateRenderObject()
    {
        return new RenderPointerListener()
        {
            OnPointerDown = OnPointerDown,
            OnPointerUp = OnPointerUp,
            OnPointerMove = OnPointerMove,
            Behaviour = Behaviour,
        };
    }

    public override void UpdateRenderObject(RenderPointerListener renderObject)
    {
        renderObject.OnPointerDown = OnPointerDown;
        renderObject.OnPointerUp = OnPointerUp;
        renderObject.OnPointerMove = OnPointerMove;
        renderObject.Behaviour = Behaviour;
    }
}
