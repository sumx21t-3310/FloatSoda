using FloatSoda.Abstractions.Input;

namespace FloatSoda.Gesture;

/// <summary>
/// ポインタIDごとに購読者を登録し、後続イベント（Down/Move/Up）を配送する。
/// 最初のヒットテストは Down 位置でしか行われないため、ドラッグ中に指がウィジェット外へ
/// 出ても認識器へイベントを届け続けるための経路を担う。Flutter の <c>PointerRouter</c> に相当。
/// </summary>
public sealed class PointerRouter
{
    private readonly Dictionary<int, List<Action<PointerEvent>>> _routes = [];

    /// <summary>ポインタに対する購読を追加する。</summary>
    public void AddRoute(int pointer, Action<PointerEvent> route)
    {
        if (!_routes.TryGetValue(pointer, out var list))
        {
            list = [];
            _routes[pointer] = list;
        }

        list.Add(route);
    }

    /// <summary>ポインタに対する購読を解除する。</summary>
    public void RemoveRoute(int pointer, Action<PointerEvent> route)
    {
        if (!_routes.TryGetValue(pointer, out var list)) return;

        list.Remove(route);
        if (list.Count == 0) _routes.Remove(pointer);
    }

    /// <summary>イベントを、そのポインタの全購読者へ配送する。</summary>
    public void Route(PointerEvent pointerEvent)
    {
        if (!_routes.TryGetValue(pointerEvent.PointerId, out var list)) return;

        // 配送中に購読が変化（StopTracking 等）しても安全なようスナップショットを走査する。
        foreach (var route in list.ToArray())
        {
            route(pointerEvent);
        }
    }
}
