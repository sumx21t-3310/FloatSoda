using FloatSoda.Abstractions.Input;
using FloatSoda.Gesture;
using FloatSoda.RenderObjects.Gesture;

namespace FloatSoda.Core;

/// <summary>
/// ポインターごとの前回ヒットパスを保持し、パス差分からEnterとExitを合成します。
/// </summary>
internal sealed class PointerHoverTracker
{
    private readonly Dictionary<int, IReadOnlyList<HitTestEntry>> _lastHitPaths = [];

    /// <summary>
    /// 現在のヒットパスと前回のパスを比較し、対象となるListenerへExit、Enterの順で通知します。
    /// </summary>
    /// <param name="pointerEvent">差分計算の基準となる位置とポインター識別子を持つイベント。</param>
    /// <param name="hitTestResult">現在位置のヒットパス。入力領域から削除された場合は<see langword="null"/>です。</param>
    public void Update(PointerEvent pointerEvent, HitTestResult? hitTestResult)
    {
        var pointerId = pointerEvent.PointerId;
        var previous = _lastHitPaths.GetValueOrDefault(pointerId) ?? Array.Empty<HitTestEntry>();
        var current = hitTestResult?.Path
            .Where(entry => entry.Target is RenderPointerListener)
            .ToArray() ?? Array.Empty<HitTestEntry>();

        foreach (var entry in previous)
        {
            if (ContainsTarget(current, entry.Target)) continue;
            Dispatch(pointerEvent with { Phase = PointerEventPhase.Exit }, entry);
        }

        for (var i = current.Length - 1; i >= 0; i--)
        {
            var entry = current[i];
            if (ContainsTarget(previous, entry.Target)) continue;
            Dispatch(pointerEvent with { Phase = PointerEventPhase.Enter }, entry);
        }

        if (current.Length == 0)
        {
            _lastHitPaths.Remove(pointerId);
        }
        else
        {
            _lastHitPaths[pointerId] = current;
        }
    }

    private static bool ContainsTarget(IEnumerable<HitTestEntry> entries, IHitTestTarget target)
        => entries.Any(entry => ReferenceEquals(entry.Target, target));

    private static void Dispatch(PointerEvent pointerEvent, HitTestEntry entry)
        => entry.Target.HandleEvent(pointerEvent with { Transform = entry.Transform }, entry);
}
