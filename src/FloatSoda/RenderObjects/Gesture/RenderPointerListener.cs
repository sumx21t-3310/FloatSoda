using FloatSoda.Abstractions.Geometries;
using FloatSoda.Abstractions.Input;
using FloatSoda.Gesture;

namespace FloatSoda.RenderObjects.Gesture;

public class RenderPointerListener : RenderProxyBox
{
    public Action<PointerEvent>? OnPointerDown { get; set; }
    public Action<PointerEvent>? OnPointerUp { get; set; }
    public Action<PointerEvent>? OnPointerMove { get; set; }

    /// <summary>ヒットテストでの振る舞い。空白領域を掴めるか／背後へ通すかを決める。</summary>
    public HitTestBehaviour Behaviour { get; set; } = HitTestBehaviour.DeferToChild;

    public override bool HitTestSelf(Offset position) => Behaviour == HitTestBehaviour.Opaque;

    public override bool HitTest(HitTestResult result, Offset position)
    {
        if (!Size.Contains(position)) return false;

        var hit = HitTestChildren(result, position) || HitTestSelf(position);

        // Translucent は自分をパスへ載せつつ、hit=false を返して背後の兄弟にも探索を続けさせる。
        if (hit || Behaviour == HitTestBehaviour.Translucent)
        {
            result.Add(new HitTestEntry(this));
        }

        return hit;
    }

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
