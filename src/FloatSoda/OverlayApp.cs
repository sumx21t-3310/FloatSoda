using System.Diagnostics;
using System.Runtime.InteropServices;
using FloatSoda.Exceptions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using Valve.VR;

namespace FloatSoda;

public class OverlayBackgroundService(ILogger<OverlayBackgroundService> logger, EventDispatcher eventDispatcher, Renderer renderer, Element root) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            Initialize();
            await Update(stoppingToken);
        }
        finally
        {
            Shutdown();
        }
    }

    private async Task Update(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessEvents(stoppingToken);
            await renderer.Render(root);
        }
    }

    private async Task ProcessEvents(CancellationToken stoppingToken)
    {
        if (OpenVR.System == null) return;

        var vrEvent = new VREvent_t();
        var size = (uint)Marshal.SizeOf<VREvent_t>();

        while (OpenVR.System.PollNextEvent(ref vrEvent, size) && !stoppingToken.IsCancellationRequested)
        {
            var eventType = (EVREventType)vrEvent.eventType;

            switch (eventType)
            {
                case EVREventType.VREvent_Quit or EVREventType.VREvent_ProcessQuit:
                    logger.LogInformation($"Steam VR quit event received. ");

                    return;

                default:
                    eventDispatcher.Dispatch(vrEvent);
                    break;
            }

            await Task.Yield();
        }
    }

    private void Initialize()
    {
        if (OpenVR.System == null)
        {
            var error = EVRInitError.None;
            OpenVR.Init(ref error, EVRApplicationType.VRApplication_Overlay);
            error.ThrowIfError();
        }

        // renderer.Initialize();
    }


    private void Shutdown()
    {
        if (OpenVR.System != null) return;
        OpenVR.System?.AcknowledgeQuit_Exiting();
        OpenVR.Shutdown();
    }
}

public class EventDispatcher
{
    private readonly Dictionary<EVREventType, Action<VREvent_t>> _handler = new();
    public void Register(EVREventType type, Action<VREvent_t> handler) => _handler[type] = handler;

    public void Dispatch(VREvent_t vrEvent)
    {
        var eventType = (EVREventType)vrEvent.eventType;
        if (_handler.TryGetValue(eventType, out var action))
        {
            action.Invoke(vrEvent);
        }
    }
}

public abstract class Element(Element? child)
{
    public Element? Child { get; } = child;

    public void Draw(SKCanvas canvas)
    {
        OnDraw(canvas);
        Child?.Draw(canvas);
    }

    public bool IsDirty { get; } = true;

    protected abstract void OnDraw(SKCanvas canvas);
}

public class DebugElement(Element? child = null) : Element(child)
{
    protected override void OnDraw(SKCanvas canvas)
    {
    }
}

public class Renderer(string overlayName, string overlayKey, FrameTimer frameTimer) : IDisposable
{
    private ulong _overlayHandle;
    private SKSurface _surface;
    private IntPtr _pixelBuffer;

    public void Initialize(int width = 1024, int height = 1024)
    {
        OpenVR.Overlay.CreateOverlay(overlayKey, overlayName, ref _overlayHandle).ThrowIfError();
        OpenVR.Overlay.SetOverlayWidthInMeters(_overlayHandle, 1.0f);
        OpenVR.Overlay.ShowOverlay(_overlayHandle);

        var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);

        _pixelBuffer = Marshal.AllocHGlobal(info.BytesSize);

        _surface = SKSurface.Create(info, _pixelBuffer, info.RowBytes);
    }


    public async Task Render(Element root)
    {
        // OnRender(root);
        await frameTimer.WaitForNextFrame();
    }

    private void OnRender(Element root)
    {
        var canvas = _surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        root.Draw(canvas);
        canvas.Flush();

        var textureT = new Texture_t
        {
            handle = _pixelBuffer,
            eType = ETextureType.OpenGL,
            eColorSpace = EColorSpace.Auto
        };

        OpenVR.Overlay.SetOverlayTexture(_overlayHandle, ref textureT).ThrowIfError();
    }

    public void Dispose()
    {
        _surface.Dispose();
        if (_pixelBuffer != IntPtr.Zero) Marshal.FreeHGlobal(_pixelBuffer);

        if (_overlayHandle != 0) OpenVR.Overlay.DestroyOverlay(_overlayHandle);
    }
}

public class FrameTimer
{
    private readonly Stopwatch _stopwatch = new();
    public FrameTimer() => _stopwatch.Start();

    public float DeltaTime { get; private set; }

    public async Task WaitForNextFrame(float targetFrameRate = 60)
    {
        var targetMs = 1000.0 / targetFrameRate;

        var elapsedMs = _stopwatch.Elapsed.TotalMilliseconds;
        var delay = (int)(targetMs - elapsedMs);

        if (delay > 0)
        {
            await Task.Delay(delay);
        }

        DeltaTime = (float)(_stopwatch.Elapsed.TotalSeconds);
        _stopwatch.Restart();
    }
}