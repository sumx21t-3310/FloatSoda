using FloatSoda.Engine.Abstractions.Scheduling;
using Microsoft.Extensions.Logging;

namespace FloatSoda.Engine.Test;

public class PostTaskRunnerTest
{
    private sealed class NoopFramePacer : IFramePacer
    {
        public void WaitForNextFrame(CancellationToken cancellationToken = default) { }
    }

    /// <summary>ログに記録されたエラーと警告の件数を数えるだけのロガー。</summary>
    private sealed class CountingLogger : ILogger
    {
        public int ErrorCount { get; private set; }
        public int WarningCount { get; private set; }

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter
        )
        {
            if (logLevel == LogLevel.Error)
                ErrorCount++;
            if (logLevel == LogLevel.Warning)
                WarningCount++;
        }
    }

    /// <summary>
    /// 更新の途中で停止を要求するためのサブクラス。<see cref="Update"/> に入ったことを通知し、
    /// 渡された処理を実行してから戻る。専用スレッドの終了は <see cref="Stopped"/> で待てる。
    /// </summary>
    private sealed class BlockingPostTaskRunner(
        IFramePacer pacer,
        ILogger logger,
        Action<BlockingPostTaskRunner> onUpdate
    ) : PostTaskRunner("Test", pacer, logger)
    {
        public ManualResetEventSlim InUpdate { get; } = new();
        public ManualResetEventSlim Stopped { get; } = new();

        protected override void Update()
        {
            InUpdate.Set();
            onUpdate(this);
        }

        protected override void OnStop() => Stopped.Set();
    }

    /// <summary>
    /// 更新のたびに、<see cref="FramePacer"/> と同じくキャンセルトークンの WaitHandle で待つペーサー。
    /// FramePacer は更新が1フレームより長引くと WaitHandle に触れないため、待ち方を固定する。
    /// NoopFramePacer はトークンに触れないので、破棄済みのトークンに触れる不具合を検出できない。
    /// </summary>
    private sealed class WaitHandleFramePacer : IFramePacer
    {
        public void WaitForNextFrame(CancellationToken cancellationToken = default) =>
            cancellationToken.WaitHandle.WaitOne(TimeSpan.FromMilliseconds(10));
    }

    private static IFramePacer CreateWaitingPacer() => new WaitHandleFramePacer();

    [Fact]
    public void Stop_更新中に外部スレッドから停止_エラーを記録しない()
    {
        var logger = new CountingLogger();
        using var parent = new CancellationTokenSource();
        // Stop が実行状態を下ろすまで更新に留まり、Stop のロックが外れる(Cancel が終わる)のを待ってから戻る。
        // これで、Stop の処理が更新の途中に必ず重なる。
        var runner = new BlockingPostTaskRunner(
            CreateWaitingPacer(),
            logger,
            r =>
            {
                Assert.True(SpinWait.SpinUntil(() => !r.IsRunning, TimeSpan.FromSeconds(5)));
                lock (r) { }
            }
        );

        runner.Start(parent.Token);
        Assert.True(runner.InUpdate.Wait(TimeSpan.FromSeconds(5)));
        // FloatSodaApp.Dispose と同じく、親トークンをキャンセルしてから停止する
        parent.Cancel();
        runner.Stop();

        Assert.True(runner.Stopped.IsSet);
        Assert.Equal(0, logger.ErrorCount);
    }

    [Fact]
    public void Stop_専用スレッド自身から停止_エラーを記録しない()
    {
        var logger = new CountingLogger();
        var runner = new BlockingPostTaskRunner(CreateWaitingPacer(), logger, r => r.Stop());

        runner.Start(CancellationToken.None);

        Assert.True(runner.Stopped.Wait(TimeSpan.FromSeconds(5)));
        Assert.Equal(0, logger.ErrorCount);
    }

    [Fact]
    public void Stop_終了待機がタイムアウト_スレッド終了後もエラーを記録しない()
    {
        var logger = new CountingLogger();
        using var release = new ManualResetEventSlim();
        var runner = new BlockingPostTaskRunner(CreateWaitingPacer(), logger, _ => release.Wait());

        runner.Start(CancellationToken.None);
        Assert.True(runner.InUpdate.Wait(TimeSpan.FromSeconds(5)));
        runner.Stop(); // 更新が終わらないため、3秒の終了待機がタイムアウトする

        Assert.Equal(1, logger.WarningCount);
        Assert.False(runner.Stopped.IsSet);

        release.Set();

        Assert.True(runner.Stopped.Wait(TimeSpan.FromSeconds(5)));
        Assert.Equal(0, logger.ErrorCount);
    }

    [Fact]
    public void Start_終了待機のタイムアウト後に再開始_古いスレッドの終了で新しい実行を止めない()
    {
        var logger = new CountingLogger();
        using var release = new ManualResetEventSlim();
        var updates = 0;
        var runner = new BlockingPostTaskRunner(
            CreateWaitingPacer(),
            logger,
            _ =>
            {
                // 最初の更新だけ止め、Stop の終了待機をタイムアウトさせる
                if (Interlocked.Increment(ref updates) == 1)
                    release.Wait();
            }
        );

        runner.Start(CancellationToken.None);
        Assert.True(runner.InUpdate.Wait(TimeSpan.FromSeconds(5)));
        runner.Stop();
        Assert.Equal(1, logger.WarningCount);

        runner.Start(CancellationToken.None);
        release.Set();

        // 古いスレッドの終了処理が済んだあとも、新しいスレッドは更新を続ける
        Assert.True(runner.Stopped.Wait(TimeSpan.FromSeconds(5)));
        var afterOldStopped = Volatile.Read(ref updates);
        Assert.True(
            SpinWait.SpinUntil(
                () => Volatile.Read(ref updates) > afterOldStopped + 1,
                TimeSpan.FromSeconds(5)
            )
        );
        Assert.True(runner.IsRunning);

        runner.Stop();
        Assert.Equal(0, logger.ErrorCount);
    }

    /// <summary>GLFW を起動せずに <see cref="PostTaskRunner.DrainPendingTasks"/> だけを検証するための最小サブクラス。</summary>
    private sealed class TestPostTaskRunner(ILogger? logger)
        : PostTaskRunner("Test", new NoopFramePacer(), logger)
    {
        protected override void Update() { }

        public void Drain() => DrainPendingTasks();
    }

    [Fact]
    public void DrainPendingTasks_タスクが例外を投げる_後続タスクは実行される()
    {
        var logger = new CountingLogger();
        var runner = new TestPostTaskRunner(logger);

        var ran = new List<int>();
        runner.PostTask(() => ran.Add(1));
        runner.PostTask(() => throw new InvalidOperationException("boom"));
        runner.PostTask(() => ran.Add(2));

        runner.Drain();

        // 例外を投げたタスクの後続も実行され、失敗はログに記録される
        Assert.Equal([1, 2], ran);
        Assert.Equal(1, logger.ErrorCount);
    }

    [Fact]
    public void DrainPendingTasks_複数タスクを投稿_FIFO順に実行される()
    {
        var runner = new TestPostTaskRunner(logger: null);

        var order = new List<int>();
        for (var i = 0; i < 5; i++)
        {
            var captured = i;
            runner.PostTask(() => order.Add(captured));
        }

        runner.Drain();

        Assert.Equal([0, 1, 2, 3, 4], order);
    }
}
