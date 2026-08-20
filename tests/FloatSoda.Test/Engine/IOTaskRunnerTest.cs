using FloatSoda.Engine;

namespace FloatSoda.Test.Engine;

public class IOTaskRunnerTest
{
    [Fact]
    public void IOTaskRunner_ITaskRunnerを実装する()
    {
        using var concreteRunner = new IOTaskRunner();
        ITaskRunner runner = concreteRunner;

        Assert.IsType<IOTaskRunner>(runner);
    }

    [Fact]
    public async Task RunAsync_開始済み_専用スレッドで実行する()
    {
        using var runner = new IOTaskRunner("TestIOThread");
        var callerThreadId = Environment.CurrentManagedThreadId;
        runner.Start();

        var result = await runner.RunAsync(() => new
        {
            ThreadId = Environment.CurrentManagedThreadId,
            ThreadName = Thread.CurrentThread.Name,
            IsRunnerThread = runner.RunsTasksOnCurrentThread
        });

        Assert.NotEqual(callerThreadId, result.ThreadId);
        Assert.Equal("TestIOThread", result.ThreadName);
        Assert.True(result.IsRunnerThread);
    }

    [Fact]
    public async Task RunAsync_複数の処理_FIFO順に一件ずつ実行する()
    {
        using var runner = new IOTaskRunner();
        using var firstStarted = new ManualResetEventSlim();
        using var releaseFirst = new ManualResetEventSlim();
        var order = new List<int>();
        runner.Start();

        var first = runner.RunAsync(() =>
        {
            firstStarted.Set();
            releaseFirst.Wait();
            order.Add(1);
        });
        Assert.True(firstStarted.Wait(TimeSpan.FromSeconds(3)));

        var second = runner.RunAsync(() => order.Add(2));
        var third = runner.RunAsync(() => order.Add(3));
        releaseFirst.Set();

        await Task.WhenAll(first, second, third);
        Assert.Equal([1, 2, 3], order);
    }

    [Fact]
    public async Task RunAsync_処理が例外_返されたTaskだけが失敗して後続処理を実行する()
    {
        using var runner = new IOTaskRunner();
        runner.Start();

        var failed = runner.RunAsync(() => throw new InvalidOperationException("test"));
        var succeeded = runner.RunAsync(() => 42);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => failed);
        Assert.Equal("test", exception.Message);
        Assert.Equal(42, await succeeded);
        Assert.True(runner.IsRunning);
    }

    [Fact]
    public async Task RunAsync_開始前にキャンセル_処理を実行せずTaskをキャンセルする()
    {
        using var runner = new IOTaskRunner();
        using var firstStarted = new ManualResetEventSlim();
        using var releaseFirst = new ManualResetEventSlim();
        using var cancellation = new CancellationTokenSource();
        var executed = false;
        runner.Start();

        var first = runner.RunAsync(() =>
        {
            firstStarted.Set();
            releaseFirst.Wait();
        });
        Assert.True(firstStarted.Wait(TimeSpan.FromSeconds(3)));

        var canceled = runner.RunAsync(() => executed = true, cancellation.Token);
        cancellation.Cancel();
        releaseFirst.Set();

        await first;
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => canceled);
        Assert.False(executed);
    }

    [Fact]
    public async Task Stop_投入済み処理_完了してから停止する()
    {
        using var runner = new IOTaskRunner();
        using var firstStarted = new ManualResetEventSlim();
        using var releaseFirst = new ManualResetEventSlim();
        var executed = false;
        runner.Start();

        var first = runner.RunAsync(() =>
        {
            firstStarted.Set();
            releaseFirst.Wait();
        });
        Assert.True(firstStarted.Wait(TimeSpan.FromSeconds(3)));
        var second = runner.RunAsync(() => executed = true);

        var stop = Task.Run(runner.Stop);
        releaseFirst.Set();
        await stop;

        await Task.WhenAll(first, second);
        Assert.True(executed);
        Assert.False(runner.IsRunning);
    }

    [Fact]
    public async Task Stop_専用スレッドから呼び出す_デッドロックせず停止する()
    {
        using var runner = new IOTaskRunner();
        runner.Start();

        await runner.RunAsync(runner.Stop).WaitAsync(TimeSpan.FromSeconds(3));

        Assert.False(runner.IsRunning);
    }

    [Fact]
    public void RunAsync_未開始_InvalidOperationExceptionを投げる()
    {
        using var runner = new IOTaskRunner();
        void Run() => runner.RunAsync(() => { });

        Assert.Throws<InvalidOperationException>(Run);
    }

    [Fact]
    public void Start_停止後_新しい専用スレッドで再開できる()
    {
        using var runner = new IOTaskRunner();
        runner.Start();
        runner.Stop();

        runner.Start();

        Assert.True(runner.IsRunning);
    }

    [Fact]
    public async Task Start_停止トークンをキャンセル_投入済み処理を完了して終了する()
    {
        using var runner = new IOTaskRunner();
        using var cancellation = new CancellationTokenSource();
        using var firstStarted = new ManualResetEventSlim();
        using var releaseFirst = new ManualResetEventSlim();
        var secondExecuted = false;
        runner.Start(cancellation.Token);

        var first = runner.RunAsync(() =>
        {
            firstStarted.Set();
            releaseFirst.Wait();
        });
        Assert.True(firstStarted.Wait(TimeSpan.FromSeconds(3)));
        var second = runner.RunAsync(() => secondExecuted = true);

        cancellation.Cancel();
        releaseFirst.Set();

        await Task.WhenAll(first, second).WaitAsync(TimeSpan.FromSeconds(3));
        Assert.True(secondExecuted);
        Assert.False(runner.IsRunning);

        runner.Start();
        Assert.Equal(42, await runner.RunAsync(() => 42));
    }

    [Fact]
    public void RunAsync_ランナーの停止トークンをキャンセル_InvalidOperationExceptionを投げる()
    {
        using var runner = new IOTaskRunner();
        using var cancellation = new CancellationTokenSource();
        runner.Start(cancellation.Token);
        cancellation.Cancel();
        void Run() => runner.RunAsync(() => { });

        Assert.Throws<InvalidOperationException>(Run);
    }
}
