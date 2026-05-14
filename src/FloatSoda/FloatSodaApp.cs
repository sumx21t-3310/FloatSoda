using System.Collections.Concurrent;
using FloatSoda.Common.Geometries;
using FloatSoda.Render;
using Microsoft.Extensions.Logging;
using OVRSharp;
using SkiaSharp;

namespace FloatSoda;

using System.Numerics;
using System.Runtime.InteropServices;
using Engine;
using OVR.Exceptions;
using Valve.VR;

public class FloatSodaApp(ILogger logger = null) : IDisposable
{
    private readonly RenderThreadRunner _renderThreadRunner = new("RenderThread", 60);
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
    public void Run(int targetFrameRate = 120)
    {
        try
        {
            Initialize();

            MainLoop(targetFrameRate);
        }
        finally
        {
            Dispose();
        }
    }

    private void MainLoop(int targetFrameRate)
    {
        var limiter = new FrameLimiter(targetFrameRate);

        while (!_disposed && !_cts.Token.IsCancellationRequested)
        {
            try
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

                foreach (var (windowKey, pipeline) in _windowKeys)
                {
                    lock (pipeline)
                    {
                        pipeline.RenderView.Child = new RenderConstrainedBox(
                            BoxConstraints.Tight(new SKSize(100, 100)),
                            new RenderColoredBox()
                        );
                        pipeline.FlushLayout();
                        pipeline.FlushPaint();
                        _renderThreadRunner.PostRender(windowKey, pipeline.RenderView.Layer);
                    }
                }

                limiter.Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine($"ループ実行中にエラーが発生しました: {e}");
                return;
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
            Console.WriteLine($"OpenVRの初期化に失敗しました: {e.Message}");
            throw;
        }
        catch (Exception e)
        {
            Console.WriteLine($"致命的な起動エラー: {e.Message}");
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