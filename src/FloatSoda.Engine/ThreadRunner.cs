using System.Collections.Concurrent;
using FloatSoda.Common.Layer;
using Microsoft.Extensions.Logging;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace FloatSoda.Engine;

public interface IThreadRunner
{
    void Start(CancellationToken token);
    void Stop();

    void PostTask(Action action);

    string ThreadName { get; }
    bool IsRunning { get; }
}

public abstract class ThreadRunner(string threadName, IFrameLimiter limiter, ILogger? logger = null)
    : IThreadRunner
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

    private readonly ConcurrentQueue<Action> _pendingTasks = new();

    public virtual void PostTask(Action action) => _pendingTasks.Enqueue(action);

    /// <summary>
    /// キューに積まれたタスクを実行する。1タスクが例外を投げても他のタスクとスレッド自体は継続する。
    /// ここで拾わないと最上位の<see cref="RunLoop"/>のcatchまで抜けて<see cref="OnStop"/>が走り、
    /// レンダースレッドが停止して以降すべての描画が止まる。
    /// </summary>
    protected void DrainPendingTasks()
    {
        while (_pendingTasks.TryDequeue(out var task))
        {
            try
            {
                task();
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "{ThreadName} でタスクの実行に失敗しました", ThreadName);
            }
        }
    }

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

public class RenderThreadRunner(string threadName, IFrameLimiter limiter, ILogger? logger = null)
    : ThreadRunner(threadName, limiter, logger)
{
    public void PostRender(IWindow window, ILayer layer)
    {
        PostTask(() =>
        {
            if (!IsRunning) return;

            window.Root = layer;
            window.Update();
        });
    }

    protected override void PreUpdate() => DrainPendingTasks();

    protected override void OnStart(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        if (!GLFW.Init()) throw new Exception("GLFWの初期化に失敗しました。");
    }

    protected override void Update() => GLFW.PollEvents();

    protected override void OnStop() => GLFW.Terminate();
}