using FloatSoda.Abstractions.Input;

namespace FloatSoda.Gesture;

/// <summary>
/// ポインター識別子ごとに購読者を登録し、後続イベント（Down/Move/Up）を配送する。
/// 最初のヒットテストは Down 位置でしか行われないため、ドラッグ中に指がウィジェット外へ
/// 出ても認識器へイベントを届け続けるための経路を担う。Flutter の <c>PointerRouter</c> に相当。
/// </summary>
public sealed class PointerRouter
{
    /// <summary>ポインター識別子ごとのイベント購読者。</summary>
    private readonly Dictionary<int, List<Action<PointerEvent>>> _routes = [];

    /// <summary>ポインターに対する購読を追加する。</summary>
    /// <param name="pointer">購読対象のポインター識別子。</param>
    /// <param name="route">イベントを受け取るコールバック。</param>
    public void AddRoute(int pointer, Action<PointerEvent> route)
    {
        if (!_routes.TryGetValue(pointer, out var list))
        {
            list = [];
            _routes[pointer] = list;
        }

        list.Add(route);
    }

    /// <summary>ポインターに対する購読を解除する。</summary>
    /// <param name="pointer">購読解除対象のポインター識別子。</param>
    /// <param name="route">解除するコールバック。</param>
    /// <remarks>最後の購読者を解除した場合は、ポインター識別子のエントリも削除します。</remarks>
    public void RemoveRoute(int pointer, Action<PointerEvent> route)
    {
        if (!_routes.TryGetValue(pointer, out var list)) return;

        list.Remove(route);
        if (list.Count == 0) _routes.Remove(pointer);
    }

    /// <summary>イベントを、そのポインターの全購読者へ配送する。</summary>
    /// <param name="pointerEvent">配送するポインターイベント。</param>
    /// <remarks>配送開始時の購読者一覧を使用するため、コールバック内で購読を変更できます。</remarks>
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
