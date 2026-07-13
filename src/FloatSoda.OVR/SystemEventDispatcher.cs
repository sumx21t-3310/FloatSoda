using System.Runtime.InteropServices;

namespace FloatSoda.OVR;

/// <summary>Handles an OpenVR event.</summary>
/// <param name="vrEvent">The event supplied by OpenVR.</param>
public delegate void VREventHandler(in VREvent_t vrEvent);

/// <summary>Routes polled OpenVR events to handlers registered by event type.</summary>
public abstract class VREventDispatcher
{
    /// <summary>The unmanaged size of <see cref="VREvent_t"/> passed to OpenVR polling methods.</summary>
    protected static readonly uint EventSize = (uint)Marshal.SizeOf<VREvent_t>();
    private readonly Dictionary<EVREventType, List<VREventHandler>> _handlers = new();

    /// <summary>Registers a handler for an OpenVR event type.</summary>
    /// <param name="eventType">The event type to observe.</param>
    /// <param name="handler">The handler to invoke.</param>
    public void Register(EVREventType eventType, VREventHandler handler)
    {
        if (!_handlers.TryGetValue(eventType, out var list))
            _handlers[eventType] = list = [];
        list.Add(handler);
    }

    /// <summary>Removes one registration of a handler for an OpenVR event type.</summary>
    /// <param name="eventType">The event type whose handler is removed.</param>
    /// <param name="handler">The handler to remove.</param>
    public void Unregister(EVREventType eventType, VREventHandler handler)
    {
        if (_handlers.TryGetValue(eventType, out var list)) list.Remove(handler);
    }

    /// <summary>Removes every handler registered for an OpenVR event type.</summary>
    /// <param name="eventType">The event type whose handlers are removed.</param>
    public void Unregister(EVREventType eventType) => _handlers.Remove(eventType);

    /// <summary>Dispatches an event to a snapshot of the matching handler list.</summary>
    /// <param name="vrEvent">The event to dispatch.</param>
    protected void Dispatch(VREvent_t vrEvent)
    {
        var eventType = (EVREventType)vrEvent.eventType;
        if (!_handlers.TryGetValue(eventType, out var list)) return;
        foreach (var handler in list.ToList()) handler(vrEvent);
    }

    /// <summary>Polls and dispatches all events currently waiting in the associated OpenVR queue.</summary>
    public abstract void PollEvents();
}

/// <summary>Polls events from the process-wide OpenVR system event queue.</summary>
public class VRSystemEventDispatcher : VREventDispatcher
{
    /// <inheritdoc />
    public override void PollEvents()
    {
        var vrEvent = new VREvent_t();
        while (OpenVR.System.PollNextEvent(ref vrEvent, EventSize))
        {
            Dispatch(vrEvent);
        }
    }
}

/// <summary>Polls events addressed to a specific overlay.</summary>
/// <param name="overlayHandle">The OpenVR overlay handle whose event queue is polled.</param>
public class OverlayEventDispatcher(ulong overlayHandle) : VREventDispatcher
{
    /// <inheritdoc />
    public override void PollEvents()
    {
        var vrEvent = new VREvent_t();
        while (OpenVR.Overlay.PollNextOverlayEvent(overlayHandle, ref vrEvent, EventSize))
        {
            Dispatch(vrEvent);
        }
    }
}
