using System.Runtime.InteropServices;
using FloatSoda.Engine;
using FloatSoda.Exceptions;
using FloatSoda.Render;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Valve.VR;

namespace FloatSoda.OVR;

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

        renderer.Initialize();
    }


    private void Shutdown()
    {
        if (OpenVR.System != null) return;
        OpenVR.System?.AcknowledgeQuit_Exiting();
        OpenVR.Shutdown();
    }
}