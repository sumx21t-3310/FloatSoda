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
    private readonly IServiceCollection _services = new ServiceCollection();

    public FloatSodaApp Build()
    {
        return new FloatSodaApp(new FrameLimiter(60));
    }
}

public class FloatSodaApp(FrameLimiter limiter, ILoggerFactory? loggerFactory = null) : IDisposable
{
    private readonly RenderThreadRunner _renderThreadRunner =
        new("RenderThread", limiter, loggerFactory?.CreateLogger<RenderThreadRunner>());

    private readonly ILogger? _logger = loggerFactory?.CreateLogger<FloatSodaApp>();
    private readonly ConcurrentDictionary<string, RenderPipeline> _windowKeys = [];
    private Application? _openVR;

    private readonly CancellationTokenSource _cts = new();
    private bool _disposed;

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

                limiter.Wait();
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
                pipeline.RenderView?.Child = new RenderPositionedBox( new RenderConstrainedBox(
                    BoxConstraints.Tight(new SKSize(100, 100)),
                    new RenderColoredBox() { Color = SKColors.Red }
                ));

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

    protected virtual void Dispose(bool disposing)
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