using FloatSoda.Engine;
using Microsoft.Extensions.Logging;

namespace FloatSoda.Test.Engine;

public class ThreadRunnerTest
{
    private sealed class NoopFrameLimiter : IFrameLimiter
    {
        public void Wait() { }
    }

    /// <summary>ログに記録されたエラー件数を数えるだけのロガー。</summary>
    private sealed class CountingLogger : ILogger
    {
        public int ErrorCount { get; private set; }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (logLevel == LogLevel.Error) ErrorCount++;
        }
    }

    /// <summary>GLFW を起動せずに <see cref="ThreadRunner.DrainPendingTasks"/> だけを検証するための最小サブクラス。</summary>
    private sealed class TestThreadRunner(ILogger? logger)
        : ThreadRunner("Test", new NoopFrameLimiter(), logger)
    {
        protected override void Update() { }

        public void Drain() => DrainPendingTasks();
    }

    [Fact]
    public void DrainPendingTasks_ThrowingTask_DoesNotStopSubsequentTasks()
    {
        var logger = new CountingLogger();
        var runner = new TestThreadRunner(logger);

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
    public void DrainPendingTasks_ExecutesTasksInFifoOrder()
    {
        var runner = new TestThreadRunner(logger: null);

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
