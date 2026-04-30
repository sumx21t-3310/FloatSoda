using System.Numerics;
using FloatSoda.Engine.OVR.Exceptions;
using Valve.VR;

namespace FloatSoda.Engine;

public class TrackingTransform
{
    public TrackingTarget TrackingTarget
    {
        get;
        set
        {
            field = value;
            _isDirty = true;
        }
    }

    public ETrackingUniverseOrigin TrackingUniverseOrigin
    {
        get;
        set
        {
            field = value;
            _isDirty = true;
        }
    }


    public Vector3 Position
    {
        get;
        set
        {
            field = value;
            _isDirty = true;
        }
    }

    public Quaternion Rotation
    {
        get;
        set
        {
            field = value;
            _isDirty = true;
        }
    }

    private bool _isDirty;

    internal void Update(ulong overlayHandle)
    {
        if (_isDirty == false) return;
        _isDirty = false;

        var matrix = ToHmdMatrix34_t();

        if (TrackingTarget == TrackingTarget.World)
        {
            OpenVR.Overlay
                .SetOverlayTransformAbsolute(overlayHandle, TrackingUniverseOrigin, ref matrix)
                .ThrowIfError();
        }
        else
        {
            var trackingDeviceIndex = TrackingTarget switch
            {
                TrackingTarget.LeftController => OpenVR.System.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.LeftHand),
                TrackingTarget.RightController => OpenVR.System.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.RightHand),
                TrackingTarget.Headset => OpenVR.k_unTrackedDeviceIndex_Hmd,
                _ => throw new ArgumentOutOfRangeException()
            };

            if (trackingDeviceIndex == OpenVR.k_unTrackedDeviceIndexInvalid) return;

            OpenVR.Overlay
                .SetOverlayTransformTrackedDeviceRelative(overlayHandle, trackingDeviceIndex, ref matrix)
                .ThrowIfError();
        }
    }

    private HmdMatrix34_t ToHmdMatrix34_t()
    {
        Matrix4x4 m = Matrix4x4.CreateFromQuaternion(Rotation);

        return new HmdMatrix34_t
        {
            // 回転成分の代入
            m0 = m.M11, m1 = m.M12, m2 = m.M13,
            m4 = m.M21, m5 = m.M22, m6 = m.M23,
            m8 = m.M31, m9 = m.M32, m10 = m.M33,

            // 位置成分の代入 (4列目)
            m3 = Position.X,
            m7 = Position.Y,
            m11 = Position.Z
        };
    }
}