using System.Runtime.InteropServices;
using FloatSoda.OVR.Overlay;

namespace FloatSoda.OVR;

public delegate void VREventHandler(in VREvent_t vrEvent);

public abstract class VREventDispatcher
{
    protected static readonly uint EventSize = (uint)Marshal.SizeOf<VREvent_t>();
    private readonly Dictionary<EVREventType, List<VREventHandler>> _handlers = new();

    public void Register(EVREventType eventType, VREventHandler handler)
    {
        if (!_handlers.TryGetValue(eventType, out var list))
            _handlers[eventType] = list = [];
        list.Add(handler);
    }

    public void Unregister(EVREventType eventType, VREventHandler handler)
    {
        if (_handlers.TryGetValue(eventType, out var list)) list.Remove(handler);
    }

    public void Unregister(EVREventType eventType) => _handlers.Remove(eventType);

    protected void Dispatch(VREvent_t vrEvent)
    {
        var eventType = (EVREventType)vrEvent.eventType;
        if (!_handlers.TryGetValue(eventType, out var list)) return;
        foreach (var handler in list.ToList()) handler(vrEvent);
    }

    public abstract void PollEvents();
}

public class VRSystemEventDispatcher : VREventDispatcher
{
    public override void PollEvents()
    {
        var vrEvent = new VREvent_t();
        while (OpenVR.System.PollNextEvent(ref vrEvent, EventSize))
        {
            Dispatch(vrEvent);
        }
    }
}

public class OverlayEventDispatcher(IOverlayIdentity identity) : VREventDispatcher
{
    public override void PollEvents()
    {
        var vrEvent = new VREvent_t();
        while (OpenVR.Overlay.PollNextOverlayEvent(identity.Handle, ref vrEvent, EventSize))
        {
            Dispatch(vrEvent);
        }
    }
}