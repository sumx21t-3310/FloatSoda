using FloatSoda.OVR.Exceptions;

namespace FloatSoda.OVR.Overlay;

/// <summary>Owns the SteamVR identity and native handle of an overlay.</summary>
public interface IOverlayIdentity : IDisposable
{
    /// <summary>Gets the stable key used to identify the overlay in SteamVR.</summary>
    string Key { get; }
    /// <summary>Gets the user-facing overlay name.</summary>
    string Name { get; }
    /// <summary>Gets the native OpenVR overlay handle.</summary>
    ulong Handle { get; }
}

/// <summary>Creates or adopts the identity of a regular OpenVR overlay.</summary>
public class OverlayIdentity : IOverlayIdentity
{
    private ulong _handle;
    /// <inheritdoc />
    public string Key { get; protected init; }
    /// <inheritdoc />
    public string Name { get; protected init; }

    /// <inheritdoc />
    public ulong Handle
    {
        get => _handle;
        private set => _handle = value;
    }

    /// <summary>Creates a new overlay identity or adopts an existing handle.</summary>
    /// <param name="key">The stable key used to identify the overlay in SteamVR.</param>
    /// <param name="name">The user-facing overlay name.</param>
    /// <param name="overlayHandle">An existing handle to adopt, or the invalid handle to create a new overlay.</param>
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

    /// <summary>Destroys the owned OpenVR overlay if it has not already been destroyed.</summary>
    public virtual void Dispose()
    {
        if (Handle != OpenVR.k_ulOverlayHandleInvalid)
        {
            OpenVR.Overlay.DestroyOverlay(Handle).ThrowIfError();
            Handle = OpenVR.k_ulOverlayHandleInvalid;
        }
    }
}

/// <summary>Owns the main and thumbnail handles of a SteamVR dashboard overlay.</summary>
public class DashboardOverlayIdentity : IOverlayIdentity
{
    /// <inheritdoc />
    public string Key { get; init; }
    /// <inheritdoc />
    public string Name { get; init; }
    /// <inheritdoc />
    public ulong Handle { get; private set; }
    /// <summary>Gets the native handle for the dashboard thumbnail.</summary>
    public ulong ThumbnailHandle { get; private set; }

    /// <summary>Creates a dashboard overlay and its thumbnail.</summary>
    /// <param name="key">The stable key used to identify the overlay in SteamVR.</param>
    /// <param name="name">The user-facing dashboard name.</param>
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

    /// <summary>Destroys the dashboard overlay if it has not already been destroyed.</summary>
    public void Dispose()
    {
        if (Handle != OpenVR.k_ulOverlayHandleInvalid)
        {
            OpenVR.Overlay.DestroyOverlay(Handle).ThrowIfError();
            Handle = OpenVR.k_ulOverlayHandleInvalid;
        }
    }
}
