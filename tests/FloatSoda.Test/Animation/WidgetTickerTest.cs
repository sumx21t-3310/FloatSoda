using FloatSoda.Animation;

namespace FloatSoda.Test.Animation;

public class WidgetTickerTest
{
    private readonly FakeFrameScheduler _scheduler = new();
    private readonly TickerProvider _provider;

    public WidgetTickerTest()
    {
        _provider = new TickerProvider { ResolveScheduler = () => _scheduler };
    }

    [Fact]
    public void Start_DeliversElapsedTimeRelativeToFirstTick()
    {
        var received = new List<TimeSpan>();
        var ticker = _provider.CreateTicker(received.Add);

        ticker.Start();

        _scheduler.Pump(TimeSpan.FromMilliseconds(100));
        _scheduler.Pump(TimeSpan.FromMilliseconds(150));

        // 初回Tickが基準点(ゼロ)になり、以後は相対時間が渡る
        Assert.Equal([TimeSpan.Zero, TimeSpan.FromMilliseconds(50)], received);
    }

    [Fact]
    public void Stop_CancelsScheduledCallback()
    {
        var tickCount = 0;
        var ticker = _provider.CreateTicker(_ => tickCount++);

        ticker.Start();
        ticker.Stop();

        Assert.Equal(0, _scheduler.PendingCount);

        _scheduler.Pump(TimeSpan.FromMilliseconds(100));

        Assert.Equal(0, tickCount);
        Assert.False(ticker.IsActive);
    }

    [Fact]
    public void Muted_PausesAndResumesTicking()
    {
        var tickCount = 0;
        var ticker = _provider.CreateTicker(_ => tickCount++);

        ticker.Start();
        ticker.Muted = true;

        Assert.Equal(0, _scheduler.PendingCount);
        Assert.True(ticker.IsActive);

        _scheduler.Pump(TimeSpan.FromMilliseconds(100));
        Assert.Equal(0, tickCount);

        ticker.Muted = false;

        _scheduler.Pump(TimeSpan.FromMilliseconds(200));
        Assert.Equal(1, tickCount);
    }

    [Fact]
    public void CreateTicker_TracksMultipleTickersIndependently()
    {
        // 旧実装はCreateTicker冒頭でClear()していたため、複数Tickerの追跡が壊れていた(回帰テスト)
        var first = 0;
        var second = 0;
        var ticker1 = _provider.CreateTicker(_ => first++);
        var ticker2 = _provider.CreateTicker(_ => second++);

        ticker1.Start();
        ticker2.Start();

        _scheduler.Pump(TimeSpan.Zero);

        Assert.Equal(1, first);
        Assert.Equal(1, second);

        ticker1.Dispose();

        _scheduler.Pump(TimeSpan.FromMilliseconds(100));

        Assert.Equal(1, first);
        Assert.Equal(2, second);
    }

    [Fact]
    public void Stop_ThenStart_ResetsElapsedTimeBase()
    {
        var received = new List<TimeSpan>();
        var ticker = _provider.CreateTicker(received.Add);

        ticker.Start();
        _scheduler.Pump(TimeSpan.FromMilliseconds(100));

        ticker.Stop();
        ticker.Start();
        _scheduler.Pump(TimeSpan.FromMilliseconds(500));

        // 再スタート後は新しい基準点からゼロで始まる
        Assert.Equal([TimeSpan.Zero, TimeSpan.Zero], received);
    }
}
