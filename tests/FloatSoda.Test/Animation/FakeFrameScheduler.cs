using FloatSoda.Core;

namespace FloatSoda.Test.Animation;

/// <summary>
/// テスト用の手動ポンプ式スケジューラ。
/// <see cref="Pump"/>に渡した時刻でWidgetBinding.BeginFrame相当の発火を再現する。
/// </summary>
public class FakeFrameScheduler : IFrameScheduler
{
    private readonly Dictionary<int, Action<TimeSpan>> _callbacks = [];
    private int _nextId;

    /// <summary>未発火のコールバック数。</summary>
    public int PendingCount => _callbacks.Count;

    public int ScheduleFrameCallback(Action<TimeSpan> callback)
    {
        _callbacks[++_nextId] = callback;
        return _nextId;
    }

    public void CancelFrameCallback(int id) => _callbacks.Remove(id);

    /// <summary>
    /// 登録済みコールバックへタイムスタンプを配って発火する。
    /// 発火中の再登録は次のPumpまで持ち越される(BeginFrameと同じスナップショット方式)。
    /// </summary>
    public void Pump(TimeSpan elapsed)
    {
        var callbacks = _callbacks.Values.ToList();
        _callbacks.Clear();

        foreach (var callback in callbacks)
        {
            callback(elapsed);
        }
    }
}
