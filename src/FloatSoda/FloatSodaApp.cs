using System.Collections.Concurrent;
using FloatSoda.Geometrics;
using FloatSoda.Render;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OVRSharp;
using SkiaSharp;

namespace FloatSoda;

using System.Numerics;
using System.Runtime.InteropServices;
using Engine;
using OVR.Exceptions;
using Valve.VR;

public class FloatSodaAppBuilder
{
    // 利用者が手軽にデフォルト構成で始められるようにするスタティックメソッド
    public static FloatSodaAppBuilder CreateDefault()
    {
        var builder = new FloatSodaAppBuilder();
        builder.Services.AddLogging(logging => { logging.AddConsole(); });

        builder.Services.AddSingleton<ILogger>(sp =>
        {
            var factory = sp.GetRequiredService<ILoggerFactory>();
            return factory.CreateLogger("FloatSoda");
        });

        builder.Services.AddScoped<IFrameLimiter, FrameLimiter>();

        return builder;
    }

    public IServiceCollection Services { get; } = new ServiceCollection();

    public FloatSodaApp Build()
    {
        var provider = Services.BuildServiceProvider();
        var limiter = provider.GetRequiredService<IFrameLimiter>();
        var loggerFactory = provider.GetService<ILoggerFactory>();
        return new FloatSodaApp(limiter, loggerFactory);
    }
}

public class FloatSodaApp : IDisposable
{
    private readonly RenderThreadRunner _renderThreadRunner;

    private readonly ILogger? _logger;
    private readonly ConcurrentDictionary<string, RenderPipeline> _windowKeys = [];
    private Application? _openVR;

    private readonly CancellationTokenSource _cts = new();
    private bool _disposed;
    private readonly IFrameLimiter _limiter;

    internal FloatSodaApp(IFrameLimiter limiter, ILoggerFactory? loggerFactory = null)
    {
        _limiter = limiter;
        _renderThreadRunner =
            new RenderThreadRunner("RenderThread", limiter, loggerFactory?.CreateLogger<RenderThreadRunner>());
        _logger = loggerFactory?.CreateLogger<FloatSodaApp>();
    }

    public void CreateOverlayWindow(
        string windowName,
        RenderPipeline pipeline,
        bool isDashboard = false,
        string? thumbnailPath = null,
        float widthInMeters = 0.5f,
        Vector3? position = null,
        Quaternion? rotation = null,
        Overlay.TrackedDeviceRole trackedDevice = Overlay.TrackedDeviceRole.None
    )
    {
        var uniqueKey = WindowKeyGenerator.GenerateKey(windowName);
        _windowKeys.TryAdd(uniqueKey, pipeline);
        _renderThreadRunner.CreateOverlayWindow(
            uniqueKey,
            windowName,
            isDashboard,
            (int)pipeline.RenderView.Size.Width,
            (int)pipeline.RenderView.Size.Height,
            thumbnailPath,
            widthInMeters,
            position,
            rotation,
            trackedDevice
        );
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
        while (_openVR?.OVRSystem?.PollNextEvent(ref vrEvent, (uint)Marshal.SizeOf<VREvent_t>()) ?? false)
        {
            var eventType = (EVREventType)vrEvent.eventType;

            switch (eventType)
            {
                case EVREventType.VREvent_Quit or EVREventType.VREvent_ProcessQuit:
                    _openVR?.OVRSystem?.AcknowledgeQuit_Exiting();
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
                pipeline.RenderView?.Child = new RenderPositionedBox
                {
                    Child = new RenderFlex
                    {
                        MainAxisAlignment = MainAxisAlignment.SpaceBetween,
                        CrossAxisAlignment = CrossAxisAlignment.Center,
                        Direction = Axis.Vertical,
                        Children =
                        [
                            new RenderConstrainedBox
                            {
                                AdditionalConstraints = BoxConstraints.Tight(300, 300),
                                Child = new RenderColoredBox() { Color = SKColors.Tomato }
                            },
                            new RenderConstrainedBox
                            {
                                AdditionalConstraints = BoxConstraints.Tight(300, 300),
                                Child = new RenderColoredBox() { Color = SKColors.Gold }
                            },
                            new RenderConstrainedBox
                            {
                                AdditionalConstraints = BoxConstraints.Tight(300, 300),
                                Child = new RenderColoredBox() { Color = SKColors.LimeGreen }
                            }
                        ]
                    }
                };

                pipeline.FlushLayout();
                pipeline.FlushPaint();

                _renderThreadRunner.PostRender(windowKey, pipeline.RenderView?.Layer.Clone());
            }
        }
    }

    private void Initialize()
    {
        try
        {
            _openVR = new Application(Application.ApplicationType.Overlay);

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

        _openVR?.Shutdown();
        _openVR = null;

        _disposed = true;
    }
}