using FloatSoda.Abstractions.Input;
using FloatSoda.Gesture;
using FloatSoda.RenderObjects.Gesture;

namespace FloatSoda.Widgets.Gesture;

/// <summary>
/// マウス、VRレーザーなどのポインターが子の領域へ出入りしたことを通知するウィジェットです。
/// </summary>
/// <remarks>
/// 押下に依存しないホバー状態を構築するために使用します。
/// Down、Move、Upも扱う場合は<see cref="Listener"/>を使用します。
/// </remarks>
/// <seealso cref="RenderPointerRegion"/>
public record PointerRegion : SingleChildRenderObjectWidget<RenderPointerRegion>
{
    /// <summary>ポインターが領域へ入ったときに呼び出されるコールバックを取得します。</summary>
    public Action<PointerEvent>? OnPointerEnter { get; init; }

    /// <summary>ポインターが領域から出たときに呼び出されるコールバックを取得します。</summary>
    public Action<PointerEvent>? OnPointerExit { get; init; }

    /// <summary>
    /// ヒットテストでの振る舞いを取得します。既定では子の空白を含む領域全体を対象にします。
    /// </summary>
    public HitTestBehaviour Behaviour { get; init; } = HitTestBehaviour.Opaque;

    /// <inheritdoc />
    public override RenderPointerRegion CreateRenderObject()
    {
        return new RenderPointerRegion
        {
            OnPointerEnter = OnPointerEnter,
            OnPointerExit = OnPointerExit,
            Behaviour = Behaviour,
        };
    }

    /// <inheritdoc />
    public override void UpdateRenderObject(RenderPointerRegion renderObject)
    {
        renderObject.OnPointerEnter = OnPointerEnter;
        renderObject.OnPointerExit = OnPointerExit;
        renderObject.Behaviour = Behaviour;
    }
}
