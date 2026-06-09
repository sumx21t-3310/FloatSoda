namespace FloatSoda.OVR.Overlay;

public interface IOverlay : IDisposable
{
    IOverlayIdentity Identity { get; }
    OverlayOpacity Opacity { get; }
    OverlayWidthInMeters WidthInMeters { get; }
    OverlayCurvature Curvature { get; }
    OverlayTexture Texture { get; }
    OverlayFlags Flags { get; }
}

public interface IOverlay<out TIdentity> : IOverlay where TIdentity : IOverlayIdentity
{
    new TIdentity Identity { get; }
}

/// <summary>
/// ダッシュボードオーバーレイ。位置はSteamVRが管理するため
/// IMovableOverlay を継承しない。
/// </summary>
public interface IDashboardOverlay : IOverlay<DashboardOverlayIdentity>
{
    OverlayTexture Thumbnail { get; }
}

public interface IMovableOverlay : IOverlay
{
    OverlayVisibility Visibility { get; }

    OverlayTransform Transform { get; }
}

public interface IMovableOverlay<out TTransform> : IMovableOverlay where TTransform : OverlayTransform
{
    new TTransform Transform { get; }
}

public class DashboardOverlay(DashboardOverlayIdentity identity) : IDashboardOverlay
{
    public void Dispose() => Identity.Dispose();
    IOverlayIdentity IOverlay.Identity => Identity;
    public DashboardOverlayIdentity Identity { get; } = identity;
    public OverlayOpacity Opacity { get; } = new(identity.Handle);
    public OverlayWidthInMeters WidthInMeters { get; } = new(identity.Handle);
    public OverlayCurvature Curvature { get; } = new(identity.Handle);
    public OverlayTexture Texture { get; } = new(identity.Handle);
    public OverlayFlags Flags { get; } = new(identity.Handle);
    public OverlayTexture Thumbnail { get; } = new(identity.ThumbnailHandle);
}

public abstract class MovableOverlay<TTransform>(OverlayIdentity identity) : IMovableOverlay< TTransform> where TTransform : OverlayTransform
{
    public void Dispose() => Identity.Dispose();

    public IOverlayIdentity Identity { get; } = identity;

    public OverlayOpacity Opacity { get; } = new(identity.Handle);
    public OverlayWidthInMeters WidthInMeters { get; } = new(identity.Handle);
    public OverlayCurvature Curvature { get; } = new(identity.Handle);
    public OverlayTexture Texture { get; } = new(identity.Handle);
    public OverlayFlags Flags { get; } = new(identity.Handle);
    public OverlayVisibility Visibility { get; } = new(identity.Handle);
    
    OverlayTransform IMovableOverlay.Transform => Transform;
    public abstract TTransform Transform { get; }
}

public class DeviceTrackedOverlay(OverlayIdentity identity) : MovableOverlay<DeviceTrackedOverlayTransform>(identity)
{
    public override DeviceTrackedOverlayTransform Transform { get; } = new(identity.Handle);
}

public class WorldSpaceOverlay(OverlayIdentity identity) : MovableOverlay<WorldOverlayTransform>(identity)
{
    public override WorldOverlayTransform Transform { get; } = new(identity.Handle);
}