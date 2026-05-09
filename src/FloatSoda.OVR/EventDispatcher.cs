using Valve.VR;

namespace FloatSoda.OVR;

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