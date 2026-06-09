using System.Numerics;
using FloatSoda.OVR.Exceptions;
using FloatSoda.OVR.Math;

namespace FloatSoda.OVR.Overlay;

public sealed class OverlayOpacity(ulong overlayHandle)
{
    public float Value
    {
        get
        {
            var value = 0.0f;
            OpenVR.Overlay.GetOverlayAlpha(overlayHandle, ref value).ThrowIfError();
            return value;
        }
        set => OpenVR.Overlay.SetOverlayAlpha(overlayHandle, value).ThrowIfError();
    }
}

public sealed class OverlayWidthInMeters(ulong overlayHandle)
{
    public float Value
    {
        get
        {
            var value = 0.0f;
            OpenVR.Overlay.GetOverlayWidthInMeters(overlayHandle, ref value).ThrowIfError();
            return value;
        }
        set => OpenVR.Overlay.SetOverlayWidthInMeters(overlayHandle, value).ThrowIfError();
    }
}

public sealed class OverlayCurvature(ulong overlayHandle)
{
    public float Value
    {
        get
        {
            var value = 0.0f;
            OpenVR.Overlay.GetOverlayCurvature(overlayHandle, ref value).ThrowIfError();
            return value;
        }
        set => OpenVR.Overlay.SetOverlayCurvature(overlayHandle, value).ThrowIfError();
    }
}

public abstract class OverlayTransform
{
    private Vector3 _position;
    private Quaternion _rotation = Quaternion.Identity;

    public Vector3 Position
    {
        get => _position;
        set
        {
            _position = value;
            Apply();
        }
    }

    public Quaternion Rotation
    {
        get => _rotation;
        set
        {
            _rotation = value;
            Apply();
        }
    }


    protected Matrix4x4 GetMatrix() =>
        Matrix4x4.CreateFromQuaternion(_rotation) * Matrix4x4.CreateTranslation(_position);

    public abstract void Apply();
}

public sealed class WorldOverlayTransform(ulong overlayHandle) : OverlayTransform
{
    public ETrackingUniverseOrigin Origin
    {
        get => _origin;
        set
        {
            _origin = value;
            Apply();
        }
    }

    private ETrackingUniverseOrigin _origin = ETrackingUniverseOrigin.TrackingUniverseStanding;


    public override void Apply()
    {
        var hmd = GetMatrix().ToHmdMatrix34_t();

        OpenVR.Overlay.SetOverlayTransformAbsolute(overlayHandle, _origin, ref hmd).ThrowIfError();
    }
}

public enum TrackedDevice
{
    LeftController,
    RightController,
    HMD
}
public sealed class DeviceTrackedOverlayTransform(ulong overlayHandle) : OverlayTransform
{
   

    public TrackedDevice Target { get; set; }

    private uint ResolveDeviceIndex() => Target switch
    {
        TrackedDevice.HMD => OpenVR.k_unTrackedDeviceIndex_Hmd,
        TrackedDevice.LeftController => OpenVR.System.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole
            .LeftHand),
        TrackedDevice.RightController => OpenVR.System.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole
            .RightHand),
        _ => OpenVR.k_unTrackedDeviceIndexInvalid
    };

    public override void Apply()
    {
        var matrix = GetMatrix().ToHmdMatrix34_t();
        OpenVR.Overlay.SetOverlayTransformTrackedDeviceRelative(overlayHandle, ResolveDeviceIndex(), ref matrix)
            .ThrowIfError();
    }
}

public sealed class OverlayTexture(ulong overlayHandle)
{
    public void FromFile(string path) => OpenVR.Overlay.SetOverlayFromFile(overlayHandle, path).ThrowIfError();

    public void FromTexture_t(Texture_t texture) => OpenVR.Overlay.SetOverlayTexture(overlayHandle, ref texture).ThrowIfError();
}

public sealed class OverlayVisibility(ulong overlayHandle)
{
    public void Show() => OpenVR.Overlay.ShowOverlay(overlayHandle).ThrowIfError();

    public void Hide() => OpenVR.Overlay.HideOverlay(overlayHandle).ThrowIfError();
}

public sealed class OverlayFlags(ulong overlayHandle)
{
    public void SetFlag(VROverlayFlags flag, bool value)
    {
        OpenVR.Overlay.SetOverlayFlag(overlayHandle, flag, value).ThrowIfError();
    }

    public bool GetFlag(VROverlayFlags flag)
    {
        bool value = false;
        OpenVR.Overlay.GetOverlayFlag(overlayHandle, flag, ref value).ThrowIfError();
        return value;
    }

    public bool this[VROverlayFlags flags]
    {
        get => GetFlag(flags);
        set => SetFlag(flags, value);
    }
}