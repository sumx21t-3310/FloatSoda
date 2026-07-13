using System.Numerics;
using FloatSoda.OVR.Exceptions;
using FloatSoda.OVR.Math;

namespace FloatSoda.OVR.Overlay;

/// <summary>Reads or changes an overlay's opacity.</summary>
/// <param name="overlayHandle">The overlay handle to control.</param>
public sealed class OverlayOpacity(ulong overlayHandle)
{
    /// <summary>Gets or sets the overlay alpha value.</summary>
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

/// <summary>Triggers haptic feedback from the laser mouse associated with an overlay.</summary>
/// <param name="overlayHandle">The overlay handle to control.</param>
public sealed class OverlayVibration(ulong overlayHandle)
{
    /// <summary>Triggers a haptic vibration.</summary>
    /// <param name="duration">How long the vibration lasts.</param>
    /// <param name="frequency">The vibration frequency in hertz.</param>
    /// <param name="amplitude">The vibration amplitude.</param>
    public void Trigger(TimeSpan duration, float frequency, float amplitude)
    {
        OpenVR.Overlay.TriggerLaserMouseHapticVibration(
            overlayHandle,
            (float)duration.TotalSeconds,
            frequency,
            amplitude
        ).ThrowIfError();
    }

    /// <summary>Triggers a predefined vibration clip.</summary>
    /// <param name="clip">The vibration parameters to use.</param>
    public void Trigger(VibrationClip clip) => Trigger(clip.Duration, clip.Frequency, clip.Amplitude);
}

/// <summary>Defines the duration, frequency, and amplitude of haptic feedback.</summary>
/// <param name="Duration">How long the vibration lasts.</param>
/// <param name="Frequency">The vibration frequency in hertz.</param>
/// <param name="Amplitude">The vibration amplitude.</param>
public readonly record struct VibrationClip(TimeSpan Duration, float Frequency = 2000, float Amplitude = 0.25f);

/// <summary>Reads or changes an overlay's physical width.</summary>
/// <param name="overlayHandle">The overlay handle to control.</param>
public sealed class OverlayWidthInMeters(ulong overlayHandle)
{
    /// <summary>Gets or sets the overlay width in meters.</summary>
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

/// <summary>Reads or changes the curvature applied to an overlay.</summary>
/// <param name="overlayHandle">The overlay handle to control.</param>
public sealed class OverlayCurvature(ulong overlayHandle)
{
    /// <summary>Gets or sets the overlay curvature.</summary>
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

/// <summary>Represents an overlay position and rotation that is applied to OpenVR when changed.</summary>
public abstract class OverlayTransform
{
    private Vector3 _position;
    private Quaternion _rotation = Quaternion.Identity;

    /// <summary>Gets or sets the overlay translation in meters.</summary>
    public Vector3 Position
    {
        get => _position;
        set
        {
            _position = value;
            Apply();
        }
    }

    /// <summary>Gets or sets the overlay orientation.</summary>
    public Quaternion Rotation
    {
        get => _rotation;
        set
        {
            _rotation = value;
            Apply();
        }
    }


    /// <summary>Builds the affine transformation matrix for the current position and rotation.</summary>
    protected Matrix4x4 GetMatrix() =>
        Matrix4x4.CreateFromQuaternion(_rotation) * Matrix4x4.CreateTranslation(_position);

    /// <summary>Writes the current transform to OpenVR.</summary>
    public abstract void Apply();
}

/// <summary>Positions an overlay at an absolute location in a tracking universe.</summary>
/// <param name="overlayHandle">The overlay handle to control.</param>
public sealed class WorldOverlayTransform(ulong overlayHandle) : OverlayTransform
{
    /// <summary>Gets or sets the tracking universe in which the transform is expressed.</summary>
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


    /// <inheritdoc />
    public override void Apply()
    {
        var hmd = GetMatrix().ToHmdMatrix34_t();

        OpenVR.Overlay.SetOverlayTransformAbsolute(overlayHandle, _origin, ref hmd).ThrowIfError();
    }
}

/// <summary>Positions an overlay relative to a tracked device.</summary>
/// <param name="overlayHandle">The overlay handle to control.</param>
public sealed class DeviceTrackedOverlayTransform(ulong overlayHandle) : OverlayTransform
{
    /// <summary>Gets or sets the logical device to which the overlay is attached.</summary>
    /// <remarks>Changing the target does not apply the transform until <see cref="Apply"/> is called.</remarks>
    public TrackedDevice Target { get; set; } = TrackedDevice.HMD;

    /// <summary>Gets whether the most recent apply was deferred because <see cref="Target"/> was disconnected.</summary>
    /// <remarks>Use this value to decide whether a device-connection event should trigger another <see cref="Apply"/>.</remarks>
    public bool PendingReapply { get; private set; }

    /// <inheritdoc />
    public override void Apply()
    {
        var deviceIndex = TrackedDeviceResolver.ResolveIndex(Target);
        if (deviceIndex == OpenVR.k_unTrackedDeviceIndexInvalid)
        {
            // Target未接続。例外にはせず、接続イベント（VREvent_TrackedDeviceActivated /
            // VREvent_TrackedDeviceRoleChanged）を受けた再適用に委ねる。
            PendingReapply = true;
            return;
        }

        var matrix = GetMatrix().ToHmdMatrix34_t();
        OpenVR.Overlay
            .SetOverlayTransformTrackedDeviceRelative(overlayHandle, deviceIndex, ref matrix)
            .ThrowIfError();

        PendingReapply = false;
    }
}

/// <summary>Sets the texture displayed by an overlay.</summary>
/// <param name="overlayHandle">The overlay handle to control.</param>
public sealed class OverlayTexture(ulong overlayHandle)
{
    /// <summary>Loads the overlay texture from an image file.</summary>
    /// <param name="path">The image path passed to OpenVR.</param>
    public void FromFile(string path) => OpenVR.Overlay.SetOverlayFromFile(overlayHandle, path).ThrowIfError();

    /// <summary>Sets the overlay texture from an OpenVR texture descriptor.</summary>
    /// <param name="texture">The texture descriptor to submit.</param>
    public void FromTexture_t(Texture_t texture) =>
        OpenVR.Overlay.SetOverlayTexture(overlayHandle, ref texture).ThrowIfError();
}

/// <summary>Shows, hides, or queries an overlay.</summary>
/// <param name="overlayHandle">The overlay handle to control.</param>
public sealed class OverlayVisibility(ulong overlayHandle)
{
    /// <summary>Shows the overlay.</summary>
    public void Show() => OpenVR.Overlay.ShowOverlay(overlayHandle).ThrowIfError();

    /// <summary>Hides the overlay.</summary>
    public void Hide() => OpenVR.Overlay.HideOverlay(overlayHandle).ThrowIfError();

    /// <summary>Gets or sets whether the overlay is visible.</summary>
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

/// <summary>Reads or changes individual OpenVR overlay flags.</summary>
/// <param name="overlayHandle">The overlay handle to control.</param>
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

    /// <summary>Gets or sets an OpenVR overlay flag.</summary>
    /// <param name="flag">The flag to read or change.</param>
    public bool this[VROverlayFlags flag]
    {
        get => GetState(flag);
        set => SetState(flag, value);
    }
}

/// <summary>Configures and queries overlay input behavior.</summary>
/// <param name="overlayHandle">The overlay handle to control.</param>
public sealed class OverlayInput(ulong overlayHandle)
{
    /// <summary>Gets whether the overlay is currently the hover target.</summary>
    public bool IsHovered => OpenVR.Overlay.IsHoverTargetOverlay(overlayHandle);

    /// <summary>Gets or sets how the overlay receives input.</summary>
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

/// <summary>Describes where a ray intersected an overlay.</summary>
/// <param name="UV">The normalized texture coordinate at the hit point.</param>
/// <param name="Normal">The surface normal at the hit point.</param>
/// <param name="Point">The hit point in tracking-space coordinates.</param>
/// <param name="Distance">The distance from the ray origin to the hit point.</param>
public readonly record struct OverlayHit(Vector2 UV, Vector3 Normal, Vector3 Point, float Distance)
{
    /// <summary>Converts OpenVR intersection results to an <see cref="OverlayHit"/>.</summary>
    /// <param name="result">The native OpenVR intersection results.</param>
    /// <returns>The converted overlay hit.</returns>
    public static OverlayHit FromVROverlayIntersectionResults(VROverlayIntersectionResults_t result) => new()
    {
        UV = new Vector2(result.vUVs.v0, result.vUVs.v1),
        Normal = new Vector3(result.vNormal.v0, result.vNormal.v1, result.vNormal.v2),
        Point = new Vector3(result.vPoint.v0, result.vPoint.v1, result.vPoint.v2),
        Distance = result.fDistance
    };
}

/// <summary>Defines a ray used to test an overlay intersection.</summary>
/// <param name="Origin">The ray origin in tracking-space coordinates.</param>
/// <param name="Direction">The ray direction.</param>
/// <param name="TrackingOrigin">The tracking universe in which the ray is expressed.</param>
public readonly record struct IntersectionParams(
    Vector3 Origin,
    Vector3 Direction,
    ETrackingUniverseOrigin TrackingOrigin)
{
    /// <summary>Converts these values to OpenVR intersection parameters.</summary>
    /// <returns>The converted native OpenVR parameters.</returns>
    public VROverlayIntersectionParams_t ToVROverlayIntersectionParams() => new()
    {
        vSource = new HmdVector3_t { v0 = Origin.X, v1 = Origin.Y, v2 = Origin.Z },
        vDirection = new HmdVector3_t { v0 = Direction.X, v1 = Direction.Y, v2 = Direction.Z },
        eOrigin = TrackingOrigin
    };
}

/// <summary>Tests rays against an overlay surface.</summary>
/// <param name="overlayHandle">The overlay handle to test.</param>
public sealed class OverlayIntersection(ulong overlayHandle)
{
    /// <summary>Computes the nearest intersection between a ray and the overlay.</summary>
    /// <param name="intersectionParams">The origin, direction, and tracking universe of the ray.</param>
    /// <returns>The hit information, or <see langword="null"/> when the ray misses.</returns>
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
