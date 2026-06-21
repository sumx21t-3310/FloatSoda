namespace FloatSoda.OVR.Overlay;

public static class TrackedDeviceResolver
{
    public static uint ResolveIndex(this TrackedDevice device) => device switch
    {
        TrackedDevice.HMD => OpenVR.k_unTrackedDeviceIndex_Hmd,
        TrackedDevice.LeftController => OpenVR.System.GetTrackedDeviceIndexForControllerRole(
            ETrackedControllerRole.LeftHand),
        TrackedDevice.RightController => OpenVR.System.GetTrackedDeviceIndexForControllerRole(
            ETrackedControllerRole.RightHand),
        _ => OpenVR.k_unTrackedDeviceIndexInvalid
    };
}

public enum TrackedDevice
{
    LeftController,
    RightController,
    HMD
}