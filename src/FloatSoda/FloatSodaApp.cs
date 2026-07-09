using FloatSoda.OVR;
using FloatSoda.OVR.Exceptions;
using System.Collections.Concurrent;
using FloatSoda.Core;
using FloatSoda.Engine;
using FloatSoda.OVR.Overlay;
using FloatSoda.Widgets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OverlayWindow = FloatSoda.Widgets.OverlayWindow;

namespace FloatSoda;

public class FloatSodaAppBuilder
{
    public static FloatSodaAppBuilder CreateDefault(string appName = "FloatSoda")
    {
        var builder = new FloatSodaAppBuilder();
        builder.Services.AddLogging(logging => { logging.AddConsole(); });

        builder.Services.AddSingleton<ILogger>(provider =>
        {
            var factory = provider.GetRequiredService<ILoggerFactory>();
            return factory.CreateLogger("FloatSoda");
        });

        builder.Services.AddScoped<IFrameLimiter, FrameLimiter>();
        builder.AppName = appName;

        return builder;
    }

    public IServiceCollection Services { get; } = new ServiceCollection();
    public string AppName { get; set; } = "FloatSoda";

    public FloatSodaApp Build()
    {
        var provider = Services.BuildServiceProvider();
        var limiter = provider.GetRequiredService<IFrameLimiter>();
        var loggerFactory = provider.GetService<ILoggerFactory>();
        return new FloatSodaApp(limiter, AppName, loggerFactory);
    }
}

public class FloatSodaApp : IDisposable
{
    public string AppName { get; init; }
    private readonly RenderThreadRunner _renderThreadRunner;

    private readonly ILogger? _logger;
    private readonly ConcurrentDictionary<string, WidgetBinding> _bindings = [];

    private Application? _openVR;
    private VRSystemEventDispatcher? _dispatcher;

    private readonly CancellationTokenSource _cts = new();
    private readonly IFrameLimiter _limiter;
    private bool _disposed;
    private readonly ConcurrentQueue<Action> _pendingTasks = new();

    private static readonly ConcurrentDictionary<FloatSodaApp, byte> ActiveApps = new();

    internal FloatSodaApp(IFrameLimiter limiter, string appName, ILoggerFactory? loggerFactory = null)
    {
        _limiter = limiter;
        AppName = appName;
        _renderThreadRunner =
            new RenderThreadRunner("RenderThread", limiter, loggerFactory?.CreateLogger<RenderThreadRunner>());
        _logger = loggerFactory?.CreateLogger<FloatSodaApp>();
        ActiveApps.TryAdd(this, 0);
    }

    /// <summary>
    /// ホットリロード（MetadataUpdateHandler）から任意スレッドで呼ばれる。
    /// 各アプリのメインループにReassembleを積み、次フレームで全Widgetツリーを再ビルドさせる。
    /// </summary>
    internal static void ScheduleReassembleAll()
    {
        foreach (var app in ActiveApps.Keys)
        {
            app._pendingTasks.Enqueue(app.Reassemble);
        }
    }

    private void Reassemble()
    {
        foreach (var (_, binding) in _bindings)
        {
            binding.Reassemble();
        }

        _logger?.LogInformation("ホットリロード: 全Widgetツリーを再ビルドします");
    }

    /// <summary>
    /// ウィンドウ定義 <see cref="WindowWidget"/> からオーバーレイウィンドウを作成します。
    /// <see cref="WindowWidget"/> はウィジェットツリーのルートとしてマウントされ、
    /// <see cref="WindowWidget.Size"/> が null の場合、オーバーレイのサイズは
    /// <see cref="InheritedWidget.Child"/> のレイアウト結果に追従します。
    /// </summary>
    public void CreateWindow(WindowWidget window)
    {
        // 現状はOpenVRオーバーレイのみ対応。デスクトップウィンドウ等は今後の派生で追加する。
        if (window is not OverlayWindow overlayWindow)
        {
            throw new NotSupportedException($"{window.GetType().Name} は未対応のウィンドウ種別です。");
        }

        _pendingTasks.Enqueue(() =>
        {
            var title = window.Title;
            var widgetBinding = new WidgetBinding();
            _bindings.TryAdd(title, widgetBinding);
            widgetBinding.EnsureInitialized(title, _renderThreadRunner, _ => overlayWindow.CreateOverlay());
            widgetBinding.AttachRootWidget(window);
            _logger?.LogInformation("{Title}を作成しました", title);
        });
    }


    [STAThread]
    public void Run()
    {
        try
        {
            Initialize();

            MainLoop();
        }
        finally
        {
            Dispose();
        }
    }


    private void MainLoop()
    {
        while (!_cts.Token.IsCancellationRequested)
        {
            try
            {
                while (_pendingTasks.TryDequeue(out var task))
                {
                    task();
                }
            }
            catch (Exception e)
            {
                _logger?.LogError("タスクエラー: {Exception}", e);
                break;
            }

            try
            {
                PollEvents();
            }
            catch (Exception e)
            {
                _logger?.LogError("イベント処理エラー: {Exception}", e);
                break;
            }

            try
            {
                DrawFrame();
            }
            catch (Exception e)
            {
                _logger?.LogError("描画エラー: {Exception}", e);
                break;
            }

            _limiter.Wait();
        }
    }

    private void Initialize()
    {
        try
        {
            _openVR = new Application(ApplicationType.Overlay);
            _dispatcher = new VRSystemEventDispatcher();

            _dispatcher.Register(EVREventType.VREvent_Quit, (in _) =>
            {
                _openVR?.OVRSystem.AcknowledgeQuit_Exiting();
                _cts.Cancel();
            });

            _dispatcher.Register(EVREventType.VREvent_ProcessQuit, (in _) => _cts.Cancel());


            _renderThreadRunner.Start(_cts.Token);
        }
        catch (OpenVRSystemException<EVRInitError> e)
        {
            _logger?.LogError("OpenVRの初期化に失敗しました: {Message}", e.Message);
            throw;
        }
        catch (Exception e)
        {
            _logger?.LogError("致命的な起動エラー: {Message}", e.Message);
            throw;
        }
    }

    private void PollEvents() => _dispatcher?.PollEvents();


    private void DrawFrame()
    {
        foreach (var (_, binding) in _bindings)
        {
            binding.DrawFrame();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed) return;

        ActiveApps.TryRemove(this, out _);

        if (disposing)
        {
            _cts.Cancel();

            _renderThreadRunner.Stop();

            _cts.Dispose();
        }

        _openVR?.Dispose();
        _openVR = null;

        _disposed = true;
    }
}