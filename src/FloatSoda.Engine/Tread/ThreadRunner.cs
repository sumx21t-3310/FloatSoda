using System.Collections.Concurrent;
using System.Numerics;
using FloatSoda.Engine.OVR;
using FloatSoda.Engine.Painting;
using FloatSoda.Engine.Render;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace FloatSoda.Engine.Tread;

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

    public void CreateFloatingWindow(string _overlayKey, string windowName, ILayer root, float width = 0.5f, Vector3? position = null, Quaternion? rotation = null, TrackingTarget trackingTarget = TrackingTarget.World)
    {
        _pendingTasks.Enqueue(() =>
        {
            var window = new FloatingWindow(_overlayKey, $"{_overlayKey}.{windowName}", new Renderer(new GLView()), root);

            window.Transform.Position = position ?? new Vector3(0, 0, 0);
            window.Transform.Rotation = rotation ?? Quaternion.Identity;
            window.Transform.TrackingTarget = trackingTarget;
            window.Width = width;

            _windows.TryAdd(_overlayKey, window);
        });
    }


    public void CreateDashboardWindow(string _overlayKey, string windowName, string iconPath, ILayer root)
    {
        _pendingTasks.Enqueue(() =>
        {
            var uniqueKey = SteamVRKeyFactory.CreateWindowKey(_overlayKey, windowName);
            var window = new DashboardWindow(uniqueKey, windowName, iconPath, new Renderer(new GLView()), root);
            _windows.TryAdd(_overlayKey, window);
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
        if (_windows.TryGetValue(key, out var window))
        {
            window.Root = layer;
        }
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