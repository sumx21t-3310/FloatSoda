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

public sealed class OverlayVibration(ulong overlayHandle)
{
    public void Trigger(TimeSpan duration, float frequency, float amplitude)
    {
        OpenVR.Overlay.TriggerLaserMouseHapticVibration(
            overlayHandle,
            (float)duration.TotalSeconds,
            frequency,
            amplitude
        ).ThrowIfError();
    }

    public void Trigger(VibrationClip clip) => Trigger(clip.Duration, clip.Frequency, clip.Amplitude);
}

public readonly record struct VibrationClip(TimeSpan Duration, float Frequency = 2000, float Amplitude = 0.25f);

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

public sealed class DeviceTrackedOverlayTransform(ulong overlayHandle) : OverlayTransform
{
    public TrackedDevice Target { get; set; } = TrackedDevice.HMD;
    
    public override void Apply()
    {
        var matrix = GetMatrix().ToHmdMatrix34_t();
        OpenVR.Overlay
            .SetOverlayTransformTrackedDeviceRelative(overlayHandle, TrackedDeviceResolver.ResolveIndex(Target),
                ref matrix)
            .ThrowIfError();
    }
}

public sealed class OverlayTexture(ulong overlayHandle)
{
    public void FromFile(string path) => OpenVR.Overlay.SetOverlayFromFile(overlayHandle, path).ThrowIfError();

    public void FromTexture_t(Texture_t texture) =>
        OpenVR.Overlay.SetOverlayTexture(overlayHandle, ref texture).ThrowIfError();
}

public sealed class OverlayVisibility(ulong overlayHandle)
{
    public void Show() => OpenVR.Overlay.ShowOverlay(overlayHandle).ThrowIfError();

    public void Hide() => OpenVR.Overlay.HideOverlay(overlayHandle).ThrowIfError();

    public bool Visible
    {
        get => OpenVR.Overlay.IsOverlayVisible(overlayHandle);
        set
        {
            if (value)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }
    }
}

public sealed class OverlayState(ulong overlayHandle)
{
    private void SetState(VROverlayFlags flag, bool value)
    {
        OpenVR.Overlay.SetOverlayFlag(overlayHandle, flag, value).ThrowIfError();
    }

    private bool GetState(VROverlayFlags flag)
    {
        bool value = false;
        OpenVR.Overlay.GetOverlayFlag(overlayHandle, flag, ref value).ThrowIfError();
        return value;
    }

    public bool this[VROverlayFlags flag]
    {
        get => GetState(flag);
        set => SetState(flag, value);
    }
}

public sealed class OverlayInput(ulong overlayHandle)
{
    public bool IsHovered => OpenVR.Overlay.IsHoverTargetOverlay(overlayHandle);

    public VROverlayInputMethod Method
    {
        get
        {
            var value = VROverlayInputMethod.None;
            OpenVR.Overlay.GetOverlayInputMethod(overlayHandle, ref value).ThrowIfError();
            return value;
        }
        set => OpenVR.Overlay.SetOverlayInputMethod(overlayHandle, value).ThrowIfError();
    }
}

public readonly record struct OverlayHit(Vector2 UV, Vector3 Normal, Vector3 Point, float Distance)
{
    public static OverlayHit FromVROverlayIntersectionResults(VROverlayIntersectionResults_t result) => new()
    {
        UV = new Vector2(result.vUVs.v0, result.vUVs.v1),
        Normal = new Vector3(result.vNormal.v0, result.vNormal.v1, result.vNormal.v2),
        Point = new Vector3(result.vPoint.v0, result.vPoint.v1, result.vPoint.v2),
        Distance = result.fDistance
    };
}

public readonly record struct IntersectionParams(
    Vector3 Origin,
    Vector3 Direction,
    ETrackingUniverseOrigin TrackingOrigin)
{
    public VROverlayIntersectionParams_t ToVROverlayIntersectionParams() => new()
    {
        vSource = new HmdVector3_t { v0 = Origin.X, v1 = Origin.Y, v2 = Origin.Z },
        vDirection = new HmdVector3_t { v0 = Direction.X, v1 = Direction.Y, v2 = Direction.Z },
        eOrigin = TrackingOrigin
    };
}

public sealed class OverlayIntersection(ulong overlayHandle)
{
    public OverlayHit? TryIntersect(IntersectionParams intersectionParams)
    {
        var param = intersectionParams.ToVROverlayIntersectionParams();

        var result = new VROverlayIntersectionResults_t();

        if (!OpenVR.Overlay.ComputeOverlayIntersection(overlayHandle, ref param, ref result))
        {
            return null;
        }

        return OverlayHit.FromVROverlayIntersectionResults(result);
    }
}