using System.Collections.Concurrent;
using FloatSoda.OVR;
using FloatSoda.OVR.Overlay;
using FloatSoda.Render;
using FloatSoda.Render.Layout;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FloatSoda;

using System.Runtime.InteropServices;
using Engine;
using OVR.Exceptions;

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

        _logger?.LogInformation($"{windowName} : {overlayKey}を作成しました");
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
        _logger?.LogInformation($"");
        while (!_disposed && !_cts.Token.IsCancellationRequested)
        {
            try
            {
                PollEvents();

                DrawFrame();

                _limiter.Wait();
            }
            catch (Exception e)
            {
                _logger?.LogError("ループ実行中にエラーが発生しました: {Exception}", e);
                return;
            }
        }
    }

    private void PollEvents()
    {
        VREvent_t vrEvent = default;
        while (_openVR?.OVRSystem.PollNextEvent(ref vrEvent, (uint)Marshal.SizeOf<VREvent_t>()) ?? false)
        {
            var eventType = (EVREventType)vrEvent.eventType;

            switch (eventType)
            {
                case EVREventType.VREvent_Quit or EVREventType.VREvent_ProcessQuit:
                    _openVR?.OVRSystem.AcknowledgeQuit_Exiting();
                    _cts.Cancel();
                    break;
            }
        }
    }

    private void DrawFrame()
    {
        foreach (var (windowKey, pipeline) in _windowKeys)
        {
            lock (pipeline)
            {
                if (!pipeline.NeedsRebuild) continue;

                pipeline.FlushLayout();
                pipeline.FlushPaint();

                _renderThreadRunner.PostTask(context =>
                {
                    if (!context.IsRunning) return;
                    if (pipeline.RenderView?.Layer == null) return;

                    if (!context.Windows.TryGetValue(windowKey, out var window))
                    {
                        return;
                    }

                    var layer = pipeline.RenderView?.Layer.Clone();

                    if (layer == null) return;

                    window.Root = layer;
                    window.Update();
                });
            }
        }
    }

    private void Initialize()
    {
        try
        {
            _openVR = new Application(ApplicationType.Overlay);

            _renderThreadRunner.Start(_cts.Token);
        }
        catch (OpenVRSystemException<EVRInitError> e)
        {
            _logger?.LogError($"OpenVRの初期化に失敗しました: {e.Message}");
            throw;
        }
        catch (Exception e)
        {
            _logger?.LogError($"致命的な起動エラー: {e.Message}");
            throw;
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