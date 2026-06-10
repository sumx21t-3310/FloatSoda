using FloatSoda.Common.Layer;
using FloatSoda.OVR;
using FloatSoda.OVR.Overlay;
using FloatSoda.Render;
using FloatSoda.Render.Layout;
using FloatSoda.Engine;
using FloatSoda.OVR.Exceptions;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FloatSoda;

public class FloatSodaAppBuilder
{
    // 利用者が手軽にデフォルト構成で始められるようにするスタティックメソッド
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
    private readonly ConcurrentDictionary<string, RenderPipeline> _windowKeys = [];
    private Application? _openVR;
    private VREventDispatcher? _dispatcher;

    private readonly CancellationTokenSource _cts = new();
    private bool _disposed;
    private readonly IFrameLimiter _limiter;

    internal FloatSodaApp(IFrameLimiter limiter, string appName, ILoggerFactory? loggerFactory = null)
    {
        _limiter = limiter;
        AppName = appName;
        _renderThreadRunner =
            new RenderThreadRunner("RenderThread", limiter, loggerFactory?.CreateLogger<RenderThreadRunner>());
        _logger = loggerFactory?.CreateLogger<FloatSodaApp>();
    }

    public void CreateOverlayWindow(string windowName, RenderBox root, int width, int height,
        Func<IOverlay> overlayFactory)
    {
        var pipeline = new RenderPipeline
        {
            RenderView = new RenderView(width, height)
            {
                Child = new RenderPositionedBox
                {
                    Child = root
                }
            }
        };

        var overlayKey = WindowKeyGenerator.GenerateKey(windowName);

        _logger?.LogInformation("{WindowName} : {OverlayKey}を作成しました", windowName, overlayKey);
        _windowKeys.TryAdd(overlayKey, pipeline);

        _renderThreadRunner.PostTask(context =>
        {
            var render = new Renderer(new GLView(width, height));
            var overlay = overlayFactory();

            var window = new OverlayWindow(overlay, render);

            context.Windows.TryAdd(overlayKey, window);
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
            _dispatcher = new VREventDispatcher(_openVR.OVRSystem);

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
        foreach (var (windowKey, pipeline) in _windowKeys)
        {
            if (!pipeline.NeedsRebuild) continue;

            ILayer? layer;
            lock (pipeline)
            {
                if (!pipeline.NeedsRebuild) continue;

                pipeline.FlushLayout();
                pipeline.FlushPaint();
                layer = pipeline.RenderView?.Layer.Clone();
            }

            if (layer == null) continue;

            var capturedKey = windowKey;
            var capturedLayer = layer;
            _renderThreadRunner.PostTask(context =>
            {
                if (!context.IsRunning) return;
                if (!context.Windows.TryGetValue(capturedKey, out var window)) return;

                window.Root = capturedLayer;
                window.Update();
            });
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