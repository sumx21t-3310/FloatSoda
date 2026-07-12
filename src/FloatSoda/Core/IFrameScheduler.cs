namespace FloatSoda.Core;

/// <summary>
/// フレームコールバックのスケジューリングを提供します。
/// 通常は <see cref="WidgetBinding"/> が実装し、Tickerがフレームごとのタイムスタンプを受け取るために使います。
/// テストではFake実装に差し替え可能です。
/// </summary>
public interface IFrameScheduler
{
    /// <summary>次フレームで呼ばれるコールバックを登録し、キャンセル用IDを返します。</summary>
    int ScheduleFrameCallback(Action<TimeSpan> callback);

    /// <summary>登録済みのフレームコールバックをキャンセルします。</summary>
    void CancelFrameCallback(int id);
}
