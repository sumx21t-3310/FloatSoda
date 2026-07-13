namespace FloatSoda.OVR.Overlay;

/// <summary>Exposes the capabilities shared by every OpenVR overlay.</summary>
public interface IOverlay : IDisposable
{
    /// <summary>Gets the identity that owns the native overlay handle.</summary>
    IOverlayIdentity Identity { get; }
    /// <summary>Gets the overlay opacity controller.</summary>
    OverlayOpacity Opacity { get; }
    /// <summary>Gets the physical-width controller.</summary>
    OverlayWidthInMeters WidthInMeters { get; }
    /// <summary>Gets the curvature controller.</summary>
    OverlayCurvature Curvature { get; }
    /// <summary>Gets the texture controller.</summary>
    OverlayTexture Texture { get; }
    /// <summary>Gets the OpenVR flag collection.</summary>
    OverlayState State { get; }
    /// <summary>Gets the haptic-feedback controller.</summary>
    OverlayVibration Vibration { get; }
    /// <summary>Gets the input configuration and state.</summary>
    OverlayInput Input { get; }

    /// <summary>Gets the event dispatcher scoped to this overlay.</summary>
    OverlayEventDispatcher EventDispatcher { get; }
}

/// <summary>Exposes common overlay capabilities with a strongly typed identity.</summary>
/// <typeparam name="TIdentity">The overlay identity type.</typeparam>
public interface IOverlay<out TIdentity> : IOverlay where TIdentity : IOverlayIdentity
{
    /// <summary>Gets the strongly typed overlay identity.</summary>
    new TIdentity Identity { get; }
}

/// <summary>
/// ダッシュボードオーバーレイ。位置はSteamVRが管理するため
/// IMovableOverlay を継承しない。
/// </summary>
public interface IDashboardOverlay : IOverlay<DashboardOverlayIdentity>
{
    /// <summary>Gets the texture controller for the dashboard thumbnail.</summary>
    OverlayTexture Thumbnail { get; }
}

/// <summary>Represents an overlay that can be positioned and explicitly shown or hidden.</summary>
public interface IMovableOverlay : IOverlay
{
    /// <summary>Gets the visibility controller.</summary>
    OverlayVisibility Visibility { get; }
    /// <summary>Gets the ray-intersection service.</summary>
    OverlayIntersection Intersection { get; }

    /// <summary>Gets the overlay transform.</summary>
    OverlayTransform Transform { get; }
}

/// <summary>Represents a movable overlay with a strongly typed transform.</summary>
/// <typeparam name="TTransform">The transform type supported by the overlay.</typeparam>
public interface IMovableOverlay<out TTransform> : IMovableOverlay where TTransform : OverlayTransform
{
    /// <summary>Gets the strongly typed overlay transform.</summary>
    new TTransform Transform { get; }
}

/// <summary>Provides capability objects for a SteamVR dashboard overlay.</summary>
/// <param name="identity">The identity that owns the dashboard handles.</param>
public class DashboardOverlay(DashboardOverlayIdentity identity) : IDashboardOverlay
{
    /// <inheritdoc />
    public void Dispose() => Identity.Dispose();
    IOverlayIdentity IOverlay.Identity => Identity;
    /// <inheritdoc />
    public DashboardOverlayIdentity Identity { get; } = identity;
    /// <inheritdoc />
    public OverlayOpacity Opacity { get; } = new(identity.Handle);
    /// <inheritdoc />
    public OverlayWidthInMeters WidthInMeters { get; } = new(identity.Handle);
    /// <inheritdoc />
    public OverlayCurvature Curvature { get; } = new(identity.Handle);
    /// <inheritdoc />
    public OverlayTexture Texture { get; } = new(identity.Handle);
    /// <inheritdoc />
    public OverlayState State { get; } = new(identity.Handle);
    /// <inheritdoc />
    public OverlayVibration Vibration { get; } = new(identity.Handle);
    /// <inheritdoc />
    public OverlayInput Input { get; } = new(identity.Handle);
    /// <inheritdoc />
    public OverlayEventDispatcher EventDispatcher { get; } = new(identity.Handle);
    /// <inheritdoc />
    public OverlayTexture Thumbnail { get; } = new(identity.ThumbnailHandle);
}

/// <summary>Provides common capability objects for an overlay with an explicit transform.</summary>
/// <typeparam name="TTransform">The transform type supported by the overlay.</typeparam>
/// <param name="identity">The identity that owns the overlay handle.</param>
public abstract class MovableOverlay<TTransform>(OverlayIdentity identity)
    : IMovableOverlay<TTransform> where TTransform : OverlayTransform
{
    /// <inheritdoc />
    public void Dispose() => Identity.Dispose();

    /// <inheritdoc />
    public IOverlayIdentity Identity { get; } = identity;

    /// <inheritdoc />
    public OverlayOpacity Opacity { get; } = new(identity.Handle);
    /// <inheritdoc />
    public OverlayWidthInMeters WidthInMeters { get; } = new(identity.Handle);
    /// <inheritdoc />
    public OverlayCurvature Curvature { get; } = new(identity.Handle);
    /// <inheritdoc />
    public OverlayTexture Texture { get; } = new(identity.Handle);
    /// <inheritdoc />
    public OverlayState State { get; } = new(identity.Handle);
    /// <inheritdoc />
    public OverlayVibration Vibration { get; } = new(identity.Handle);
    /// <inheritdoc />
    public OverlayInput Input { get; } = new(identity.Handle);
    /// <inheritdoc />
    public OverlayEventDispatcher EventDispatcher { get; } = new(identity.Handle);

    /// <inheritdoc />
    public OverlayVisibility Visibility { get; } = new(identity.Handle);
    /// <inheritdoc />
    public OverlayIntersection Intersection { get; } = new(identity.Handle);

    OverlayTransform IMovableOverlay.Transform => Transform;
    /// <inheritdoc />
    public abstract TTransform Transform { get; }
}

/// <summary>Represents an overlay positioned relative to a tracked device.</summary>
/// <param name="identity">The identity that owns the overlay handle.</param>
public class DeviceTrackedOverlay(OverlayIdentity identity) : MovableOverlay<DeviceTrackedOverlayTransform>(identity)
{
    /// <inheritdoc />
    public override DeviceTrackedOverlayTransform Transform { get; } = new(identity.Handle);
}

/// <summary>Represents an overlay positioned absolutely in a tracking universe.</summary>
/// <param name="identity">The identity that owns the overlay handle.</param>
public class WorldSpaceOverlay(OverlayIdentity identity) : MovableOverlay<WorldOverlayTransform>(identity)
{
    /// <inheritdoc />
    public override WorldOverlayTransform Transform { get; } = new(identity.Handle);
}
