using FloatSoda.Abstractions.Input;
using FloatSoda.Gesture;
using FloatSoda.RenderObjects.Gesture;

namespace FloatSoda.Widgets.Gesture;

/// <summary>子要素のヒット領域で発生した低レベルのポインターイベントを通知するウィジェットです。</summary>
/// <remarks>タップやドラッグなどの意味付けが必要な場合は<see cref="GestureDetector"/>を使用します。</remarks>
/// <seealso cref="RenderPointerListener"/>
public record Listener : SingleChildRenderObjectWidget<RenderPointerListener>
{
    /// <summary>ポインターが押されたときに呼び出されるコールバックを取得します。</summary>
    public Action<PointerEvent>? OnPointerDown { get; init; }

    /// <summary>ポインターが離されたときに呼び出されるコールバックを取得します。</summary>
    public Action<PointerEvent>? OnPointerUp { get; init; }

    /// <summary>ポインターが移動したときに呼び出されるコールバックを取得します。</summary>
    public Action<PointerEvent>? OnPointerMove { get; init; }

    /// <summary>ヒットテストでの振る舞い。既定は子がヒットした時だけ反応する <see cref="HitTestBehaviour.DeferToChild"/>。</summary>
    public HitTestBehaviour Behaviour { get; init; } = HitTestBehaviour.DeferToChild;

    /// <inheritdoc />
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

    /// <inheritdoc />
    public override void UpdateRenderObject(RenderPointerListener renderObject)
    {
        renderObject.OnPointerDown = OnPointerDown;
        renderObject.OnPointerUp = OnPointerUp;
        renderObject.OnPointerMove = OnPointerMove;
        renderObject.Behaviour = Behaviour;
    }
}
