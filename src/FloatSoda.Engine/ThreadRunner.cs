using System.Collections.Concurrent;
using System.Numerics;
using FloatSoda.Common.Layer;
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

public abstract class ThreadRunner(string threadName, int targetFramerate = 30) : IThreadRunner
{
    private Thread? _thread;
    private readonly FrameLimiter _limiter = new(targetFramerate);

    // volatile または lock を検討。ここでは簡易化のため状態管理を強化。
    private bool _isRunning;

    public bool IsRunning => _isRunning;

    public string ThreadName => threadName;

    public void Start(CancellationToken ct)
    {
        lock (this)
        {
            // すでに実行中の場合は何もしない
            if (_thread is { IsAlive: true }) return;

            _isRunning = true;
            _thread = new Thread(() => RunLoop(ct))
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
            _isRunning = false;
        }

        if (targetThread == null) return;

        // 自分自身のスレッドから Stop を呼んだ場合は Join しない（デッドロック防止）
        if (Thread.CurrentThread != targetThread)
        {
            // 注意: 外部トークンがキャンセルされない限り、ここでブロックし続ける可能性があります。
            // タイムアウト付きの Join を検討するか、外部でキャンセルを保証する必要があります。
            targetThread.Join(3000);
        }

        _thread = null;
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

                _limiter.Wait();
            }
        }
        catch (OperationCanceledException)
        {
            // 正常な中断
        }
        catch (Exception ex)
        {
            // スレッド内での未処理例外をログ出力するなどの処理を推奨
            Console.WriteLine($"Error in {ThreadName}: {ex}");
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

public class RenderThreadRunner(string threadName, int targetFramerate) : ThreadRunner(threadName, targetFramerate)
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

    public void PostRender(string key, ILayer layer)
    {
        _pendingTasks.Enqueue(() =>
        {
            if (_windows.TryGetValue(key, out var window))
            {
                window.Root = layer;
            }
            else
            {
                Console.WriteLine($"ウィンドウが見つかりませんでした: {key}");
            }
        });
    }

    protected override void OnStart(CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
        {
            return;
        }

        if (!GLFW.Init()) throw new Exception("GLFWの初期化に失敗しました。");
    }

    protected override void Update()
    {
        foreach (var window in _windows.Values)
        {
            window.Update();
        }
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