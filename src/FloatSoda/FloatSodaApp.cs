namespace FloatSoda;

using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Common.Geometries;
using Common.Layer;
using Engine;
using OVR;
using OVRSharp;
using OVRSharp.Exceptions;
using SkiaSharp;
using Valve.VR;

public class FloatSodaApp : IDisposable
{
    private string _overlayKey = SteamVRKeyFactory.CreateKeyFromAssembly();

    private readonly List<Action> _builders = [];
    private readonly RenderThreadRunner _renderThreadRunner = new("RenderThread", 60);
    private readonly List<string> _windowKeys = [];
    private Application? _openVR;
    private readonly RenderPipeline _pipeline = new();

    private readonly CancellationTokenSource _cts = new();
    private bool _disposed;

    public void CreateOverlayWindow(
        string windowName,
        bool isDashboard = false,
        float width = 0.5f,
        Vector3? position = null,
        Quaternion? rotation = null,
        TrackingTarget trackingTarget = TrackingTarget.World)
    {
        _builders.Add(() =>
        {
            var uniqueKey = SteamVRKeyFactory.CreateWindowKey(_overlayKey, windowName);
            _windowKeys.Add(uniqueKey);
            _renderThreadRunner.CreateOverlayWindow(uniqueKey, windowName, isDashboard, width, position, rotation, trackingTarget);
        });
    }


    [STAThread]
    public void Run(int targetFrameRate = 60)
    {
        try
        {
            try
            {
                _openVR = new Application(Application.ApplicationType.Overlay);

                foreach (var build in _builders) build();

                _renderThreadRunner.Start(_cts.Token);
            }
            catch (OpenVRSystemException<EVRInitError> e)
            {
                Console.WriteLine($"OpenVRの初期化に失敗しました: {e.Message}");
                return;
            }

            catch (Exception e)
            {
                Console.WriteLine($"致命的な起動エラー: {e.Message}");
                return;
            }


            // --- 2. メインループセクション ---
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

                    foreach (var windowKey in _windowKeys)
                    {
                        _renderThreadRunner.PostRender(windowKey, _pipeline.Render(1000, 1000));
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
        finally
        {
            Dispose();
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

public class RenderPipeline
{
    private static readonly Stopwatch Stopwatch = Stopwatch.StartNew();

    public ILayer Render(float width, float height)
    {
        var root = new ContainerLayer();

        var rect = Rect.LTWH(0, 0, width, height);
        var leaf = new PictureLayer();
        var recorder = new SKPictureRecorder();
        var canvas = recorder.BeginRecording(rect);

        var paint = new SKPaint()
        {
            Color = SKColors.Red
        };

        var sin = ((float)Math.Sin(Stopwatch.Elapsed.TotalSeconds) + 1) / 2f;
        var cos = ((float)Math.Cos(Stopwatch.Elapsed.TotalSeconds) + 1) / 2f;
        canvas.DrawCircle(cos * width, sin * height, 80f, paint);
        leaf.Picture = recorder.EndRecording();

        var opacityLayer = new OpacityLayer { Alpha = 150 };

        var opacityPictureLayer = new PictureLayer();
        var opacityRecorder = new SKPictureRecorder();
        var opacityCanvas = opacityRecorder.BeginRecording(rect);

        opacityCanvas.DrawCircle(0, 0, 60f, paint);
        opacityPictureLayer.Picture = opacityRecorder.EndRecording();

        opacityLayer.Children.Add(opacityPictureLayer);
        root.Children.Add(opacityLayer);

        root.Children.Add(leaf);

        return root;
    }
}