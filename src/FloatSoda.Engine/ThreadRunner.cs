using System.Collections.Concurrent;
using System.Numerics;
using FloatSoda.Common.Layer;
using Microsoft.Extensions.Logging;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OVRSharp;

namespace FloatSoda.Engine;

public interface IThreadRunner
{
    void Start(CancellationToken token);
    void Stop();
    string ThreadName { get; }
    bool IsRunning { get; }
}

public abstract class ThreadRunner(string threadName, FrameLimiter limiter, ILogger? logger = null) : IThreadRunner
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
            // すでに実行中の場合は何もしない
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
            _thread = null; // ← lockの中に移動
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

public class RenderThreadRunner(string threadName, FrameLimiter limiter, ILogger? logger = null)
    : ThreadRunner(threadName, limiter, logger)
{
    private readonly ConcurrentDictionary<string, IWindow> _windows = [];
    private readonly ConcurrentQueue<Action> _pendingTasks = new();

    public void CreateOverlayWindow(
        string overlayKey,
        string windowName,
        bool isDashboard,
        int width,
        int height,
        string? thumbnail = null,
        float widthInMeters = 0.5f,
        Vector3? position = null,
        Quaternion? rotation = null,
        Overlay.TrackedDeviceRole trackedDevice = Overlay.TrackedDeviceRole.None)
    {
        _pendingTasks.Enqueue(() =>
        {
            var window = new OverlayWindow(overlayKey, windowName, isDashboard, new Renderer(new GLView(width, height)),
                thumbnail);

            window.Overlay.TrackedDevice = trackedDevice;
            window.Transform.Position = position ?? Vector3.Zero;
            window.Transform.Rotation = rotation ?? Quaternion.Identity;
            window.Overlay.WidthInMeters = widthInMeters;

            _windows.TryAdd(overlayKey, window);
        });
    }


    protected override void PreUpdate()
    {
        while (_pendingTasks.TryDequeue(out var task))
        {
            task();
        }
    }

    public void PostRender(string key, ILayer? layer) => _pendingTasks.Enqueue(() =>
    {
        if (!IsRunning) return;
        if (layer == null) return;
        if (!_windows.TryGetValue(key, out var window))
        {
            Logger?.LogWarning("ウィンドウが見つかりませんでした: {Key}", key);
            return;
        }

        window.Root = layer;
        window.Update();
    });

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