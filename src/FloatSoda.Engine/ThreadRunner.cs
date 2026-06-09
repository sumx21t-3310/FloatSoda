using System.Collections.Concurrent;
using FloatSoda.Common.Layer;
using Microsoft.Extensions.Logging;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace FloatSoda.Engine;

public interface IThreadRunner<TContext>
{
    void Start(CancellationToken token);
    void Stop();

    void PostTask(Action<TContext> action);

    string ThreadName { get; }
    bool IsRunning { get; }
}

public abstract class ThreadRunner<TContext>(string threadName, IFrameLimiter limiter, ILogger? logger = null)
    : IThreadRunner<TContext>
{
    private Thread? _thread;

    protected ILogger? Logger => logger;

    private volatile bool _isRunning;
    private CancellationTokenSource? _linkedTokenSource;

    public bool IsRunning => _isRunning && (_thread?.IsAlive ?? false);


    public string ThreadName => threadName;

    public void Start(CancellationToken ct)
    {
        lock (this)
        {
            if (_thread is { IsAlive: true }) return;

            _isRunning = true;
            var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            _linkedTokenSource = cts;

            _thread = new Thread(() => RunLoop(cts.Token))
            {
                IsBackground = true,
                Name = ThreadName
            };

            _thread.Start();
        }
    }

    public void Stop()
    {
        Thread? targetThread;
        lock (this)
        {
            targetThread = _thread;
            _thread = null;
            _isRunning = false;
            _linkedTokenSource?.Cancel();
            _linkedTokenSource?.Dispose();
            _linkedTokenSource = null;
        }

        if (targetThread == null) return;

        if (Thread.CurrentThread == targetThread) return;

        if (!targetThread.Join(3000))
        {
            Logger?.LogWarning("{ThreadName} の停止がタイムアウトしました", ThreadName);
        }
    }

    public abstract void PostTask(Action<TContext> action);

    private void RunLoop(CancellationToken ct)
    {
        try
        {
            OnStart(ct);

            // _isRunning フラグとトークンの両方をチェック
            while (_isRunning && !ct.IsCancellationRequested)
            {
                PreUpdate();
                Update();
                PostUpdate();

                limiter.Wait();
            }
        }
        catch (OperationCanceledException)
        {
            // 正常な中断
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error in {ThreadName}", ThreadName);
        }
        finally
        {
            OnStop();
            _isRunning = false;
        }
    }

    protected virtual void OnStart(CancellationToken ct)
    {
    }

    protected virtual void PreUpdate()
    {
    }

    protected abstract void Update();

    protected virtual void PostUpdate()
    {
    }

    protected virtual void OnStop()
    {
    }
}

public record struct RenderThreadContext(ConcurrentDictionary<string, IWindow> Windows, bool IsRunning = true);

public class RenderThreadRunner(string threadName, IFrameLimiter limiter, ILogger? logger = null)
    : ThreadRunner<RenderThreadContext>(threadName, limiter, logger)
{
    private readonly ConcurrentDictionary<string, IWindow> _windows = [];
    private readonly ConcurrentQueue<Action> _pendingTasks = new();

    public void PostRender(string key, ILayer? layer)
    {
        PostTask(context =>
        {
            if (!context.IsRunning) return;
            if (layer == null) return;
            if (!context.Windows.TryGetValue(key, out var window))
            {
                Logger?.LogWarning("ウィンドウが見つかりませんでした: {Key}", key);
                return;
            }

            window.Root = layer;
            window.Update();
        });
    }

    public override void PostTask(Action<RenderThreadContext> action)
    {
        _pendingTasks.Enqueue(() => action(new RenderThreadContext(_windows)));
    }


    protected override void PreUpdate()
    {
        while (_pendingTasks.TryDequeue(out var task))
        {
            task();
        }
    }

    protected override void OnStart(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        if (!GLFW.Init()) throw new Exception("GLFWの初期化に失敗しました。");
    }

    protected override void Update()
    {
        GLFW.PollEvents();
        GLFW.WaitEventsTimeout(1000);
    }

    protected override void OnStop()
    {
        foreach (var window in _windows.Values)
        {
            window.Dispose();
        }

        GLFW.Terminate();
    }
}