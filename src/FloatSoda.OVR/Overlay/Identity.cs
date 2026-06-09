using FloatSoda.OVR.Exceptions;

namespace FloatSoda.OVR.Overlay;

public interface IOverlayIdentity : IDisposable
{
    string Key { get; }
    string Name { get; }
    ulong Handle { get; }
}

public class OverlayIdentity : IOverlayIdentity
{
    private ulong _handle;
    public string Key { get; protected init; }
    public string Name { get; protected init; }

    public ulong Handle
    {
        get => _handle;
        private set => _handle = value;
    }

    public OverlayIdentity(string key, string name, ulong overlayHandle = OpenVR.k_ulOverlayHandleInvalid)
    {
        Key = key;
        Name = name;
        _handle = overlayHandle;

        if (_handle == OpenVR.k_ulOverlayHandleInvalid)
        {
            OpenVR.Overlay.CreateOverlay(Key, Name, ref _handle).ThrowIfError();
        }
    }

    public virtual void Dispose()
    {
        if (Handle != OpenVR.k_ulOverlayHandleInvalid)
        {
            OpenVR.Overlay.DestroyOverlay(Handle).ThrowIfError();
            Handle = OpenVR.k_ulOverlayHandleInvalid;
        }
    }
}

public class DashboardOverlayIdentity : IOverlayIdentity
{
    public string Key { get; init; }
    public string Name { get; init; }
    public ulong Handle { get; private set; }
    public ulong ThumbnailHandle { get; private set; }

    public DashboardOverlayIdentity(string key, string name)
    {
        Key = key;
        Name = name;

        ulong mainHandle = OpenVR.k_ulOverlayHandleInvalid;
        ulong thumbHandle = OpenVR.k_ulOverlayHandleInvalid;

        OpenVR.Overlay.CreateDashboardOverlay(Key, Name, ref mainHandle, ref thumbHandle).ThrowIfError();

        Handle = mainHandle;
        ThumbnailHandle = thumbHandle;
    }

    public void Dispose()
    {
        if (Handle != OpenVR.k_ulOverlayHandleInvalid)
        {
            OpenVR.Overlay.DestroyOverlay(Handle).ThrowIfError();
            Handle = OpenVR.k_ulOverlayHandleInvalid;
        }
    }
}