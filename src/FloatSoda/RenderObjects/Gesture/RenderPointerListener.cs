using FloatSoda.Abstractions.Geometries;
using FloatSoda.Abstractions.Input;
using FloatSoda.Gesture;

namespace FloatSoda.RenderObjects.Gesture;

/// <summary>ポインターイベントを受け取り、設定されたコールバックへ通知するRenderObjectです。</summary>
/// <seealso cref="Widgets.Gesture.Listener"/>
public class RenderPointerListener : RenderProxyBox
{
    /// <summary>ポインターが押されたときに呼び出すコールバックを取得または設定します。</summary>
    public Action<PointerEvent>? OnPointerDown { get; set; }

    /// <summary>ポインターが離されたときに呼び出すコールバックを取得または設定します。</summary>
    public Action<PointerEvent>? OnPointerUp { get; set; }

    /// <summary>ポインターが移動したときに呼び出すコールバックを取得または設定します。</summary>
    public Action<PointerEvent>? OnPointerMove { get; set; }

    /// <summary>ヒットテストでの振る舞い。空白領域を掴めるか／背後へ通すかを決める。</summary>
    public HitTestBehaviour Behaviour { get; set; } = HitTestBehaviour.DeferToChild;

    /// <summary>不透明な振る舞いの場合に、自身の領域をヒット対象として扱います。</summary>
    /// <param name="position">このRenderObjectのローカル座標で表した判定位置。</param>
    /// <returns><see cref="Behaviour"/>が<see cref="HitTestBehaviour.Opaque"/>の場合に<see langword="true"/>。</returns>
    public override bool HitTestSelf(Offset position) => Behaviour == HitTestBehaviour.Opaque;

    /// <summary><see cref="Behaviour"/>に従って自身と子要素をヒットテストします。</summary>
    /// <param name="result">ヒットした対象を格納する結果。</param>
    /// <param name="position">このRenderObjectのローカル座標で表した判定位置。</param>
    /// <returns>自身または子要素が背後への探索を停止するヒットになった場合に<see langword="true"/>。</returns>
    /// <remarks>
    /// <see cref="HitTestBehaviour.Translucent"/>の場合は自身をヒットパスへ追加しますが、
    /// 背後の兄弟要素の探索を継続するため、子要素がヒットしなければ<see langword="false"/>を返します。
    /// </remarks>
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

    /// <summary>イベントのフェーズに対応するポインターコールバックを呼び出します。</summary>
    /// <param name="pointerEvent">処理するポインターイベント。</param>
    /// <param name="entry">このRenderObjectに対応するヒットテストエントリ。</param>
    /// <remarks>AddおよびRemoveフェーズではコールバックを呼び出しません。</remarks>
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
