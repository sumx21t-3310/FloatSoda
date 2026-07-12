using System.Numerics;
using System.Runtime.InteropServices;
using FloatSoda.OVR.Math;

namespace FloatSoda.OVR.Overlay;

/// <summary>
/// <see cref="MakeOverlaysInteractiveIfVisible"/> を使わずに、コントローラーレイとオーバーレイの
/// 交差を毎フレーム手動で計算しポインターイベントを生成します。
/// SteamVR の入力横取りが発生しないため、VRChat の AFK 判定を回避できます。
/// </summary>
public sealed class ControllerPointerSystem(OverlayIntersection intersection)
{
    private static readonly uint ControllerStateSize = (uint)Marshal.SizeOf<VRControllerState_t>();

    private bool _wasPressed;

    /// <summary>
    /// 指定コントローラーのレイをオーバーレイに向けて交差判定を行い、
    /// ヒットした場合は UV 座標とフェーズを持つ <see cref="PointerData"/> を返します。
    /// ヒットしない場合は <see langword="null"/> を返します。
    /// </summary>
    public PointerData? Poll(TrackedDevice device, ETrackingUniverseOrigin trackingOrigin = ETrackingUniverseOrigin.TrackingUniverseStanding)
    {
        var deviceIndex = device.ResolveIndex();
        if (deviceIndex == OpenVR.k_unTrackedDeviceIndexInvalid)
        {
            _wasPressed = false;
            return null;
        }

        var poses = new TrackedDevicePose_t[deviceIndex + 1];
        OpenVR.System.GetDeviceToAbsoluteTrackingPose(trackingOrigin, 0f, poses);
        var pose = poses[deviceIndex];
        if (!pose.bPoseIsValid)
        {
            _wasPressed = false;
            return null;
        }

        var matrix = pose.mDeviceToAbsoluteTracking.ToMatrix4x4();

        // OpenVR の行列では列 2 (M?3) が -Z = コントローラー前方
        var origin = new Vector3(matrix.M41, matrix.M42, matrix.M43);
        var forward = Vector3.Normalize(new Vector3(-matrix.M13, -matrix.M23, -matrix.M33));

        var hit = intersection.TryIntersect(new IntersectionParams(origin, forward, trackingOrigin));
        if (hit is null)
        {
            _wasPressed = false;
            return null;
        }

        var state = new VRControllerState_t();
        OpenVR.System.GetControllerState(deviceIndex, ref state, ControllerStateSize);
        var isPressed = (state.ulButtonPressed & TriggerMask) != 0;

        var change = (isPressed, _wasPressed) switch
        {
            (true, false) => PointerChange.Down,
            (false, true) => PointerChange.Up,
            _ => PointerChange.Hover,
        };
        _wasPressed = isPressed;

        return new PointerData(hit.Value.UV.X, hit.Value.UV.Y, change);
    }



    private static readonly ulong TriggerMask = 1UL << (int)EVRButtonId.k_EButton_SteamVR_Trigger;
}
