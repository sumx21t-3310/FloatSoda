using System.Runtime.InteropServices;

namespace FloatSoda.OVR;

public delegate void VREventHandler(in VREvent_t vrEvent);

public class VREventDispatcher(CVRSystem system)
{
    private static readonly uint EventSize = (uint)Marshal.SizeOf<VREvent_t>();
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

    private void Dispatch(VREvent_t vrEvent)
    {
        var eventType = (EVREventType)vrEvent.eventType;
        if (!_handlers.TryGetValue(eventType, out var list)) return;
        foreach (var handler in list.ToList()) handler(vrEvent);
    }

    public void PollEvents()
    {
        var vrEvent = new VREvent_t();
        while (system.PollNextEvent(ref vrEvent, EventSize))
        {
            Dispatch(vrEvent);
        }
    }
}