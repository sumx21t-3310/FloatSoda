using FloatSoda.Core;

namespace FloatSoda.Animation;

/// <summary>
/// フレームごとに「開始からの相対時間」を通知するTicker。
/// 自分では時計を持たず、<see cref="IFrameScheduler"/>(通常はWidgetBinding)から
/// フレームコールバック経由でタイムスタンプを受け取ります。FlutterのTicker相当。
/// </summary>
public class WidgetTicker(
    Action<TimeSpan> onTick,
    ITickerProvider creator,
    Func<IFrameScheduler?> resolveScheduler) : IDisposable
{
    /// <summary>Start済みかどうか。Mutedでも進行中扱いのままです。</summary>
    public bool IsActive { get; private set; }

    private int? _animationId = null;

    /// <summary>次フレームのコールバックを登録すべき状態かどうか。</summary>
    public bool ShouldScheduledTick => IsActive && !Muted && !Scheduled;

    /// <summary>初回Tickのタイムスタンプ。相対時間の基準点になります。</summary>
    public TimeSpan? StartTime { get; private set; }

    /// <summary>
    /// trueの間はTickを一時停止します。IsActiveは維持されるため、
    /// falseに戻すと(経過時間の基準はそのままで)再開します。
    /// </summary>
    public bool Muted
    {
        get;
        set
        {
            if (field == value) return;
            field = value;

            if (value)
            {
                UnscheduleTick();
            }
            else if (ShouldScheduledTick)
            {
                ScheduleTick();
            }
        }
    }

    /// <summary>Tickの供給を開始します。経過時間は次のTickを基準にゼロから始まります。</summary>
    public void Start()
    {
        if (IsActive) return;

        IsActive = true;
        if (ShouldScheduledTick) ScheduleTick();
    }

    /// <summary>Tickの供給を停止し、経過時間の基準点をリセットします。</summary>
    public void Stop()
    {
        if (!IsActive) return;

        IsActive = false;
        StartTime = null;

        UnscheduleTick();
    }


    private void Tick(TimeSpan timeStep)
    {
        _animationId = null;
        StartTime ??= timeStep;

        onTick(timeStep - StartTime.Value);

        if (ShouldScheduledTick) ScheduleTick();
    }

    private void ScheduleTick()
    {
        _animationId = resolveScheduler()?.ScheduleFrameCallback(Tick);
    }

    private void UnscheduleTick()
    {
        if (!Scheduled) return;

        resolveScheduler()?.CancelFrameCallback(_animationId!.Value);
        _animationId = null;
    }

    /// <summary>次フレームのコールバックが登録済みかどうか。</summary>
    public bool Scheduled => _animationId != null;

    public void Dispose()
    {
        creator.RemoveTicker(this);
        IsActive = false;
        UnscheduleTick();
    }
}

/// <summary>Tickerの供給元。FlutterのTickerProvider相当。</summary>
public interface ITickerProvider
{
    WidgetTicker CreateTicker(Action<TimeSpan> onTick);
    void RemoveTicker(WidgetTicker ticker);
}

/// <summary>
/// 生成したTickerを追跡する標準の<see cref="ITickerProvider"/>実装。
/// State側で使う場合は<see cref="TickerProviderState{T}"/>を継承する方が簡単です。
/// </summary>
public class TickerProvider : ITickerProvider
{
    /// <summary>
    /// Tickerが属するウィンドウの<see cref="IFrameScheduler"/>を遅延解決します。
    /// マウント前などスケジューラ未確定の間はnullを返して構いません。
    /// </summary>
    public required Func<IFrameScheduler?> ResolveScheduler { get; init; }

    private readonly HashSet<WidgetTicker> _tickers = [];

    public WidgetTicker CreateTicker(Action<TimeSpan> onTick)
    {
        var result = new WidgetTicker(onTick, this, ResolveScheduler);

        _tickers.Add(result);

        return result;
    }

    public void RemoveTicker(WidgetTicker ticker) => _tickers.Remove(ticker);
}
