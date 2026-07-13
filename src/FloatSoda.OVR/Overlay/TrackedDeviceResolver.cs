namespace FloatSoda.OVR.Overlay;

/// <summary>Resolves logical tracked-device roles to their current OpenVR device indices.</summary>
public static class TrackedDeviceResolver
{
    /// <summary>Gets the current OpenVR index for <paramref name="device"/>.</summary>
    /// <param name="device">The logical device role to resolve.</param>
    /// <returns>The tracked-device index, or <see cref="OpenVR.k_unTrackedDeviceIndexInvalid"/> when no matching device is connected.</returns>
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

/// <summary>Identifies a tracked device by its logical role rather than a transient OpenVR index.</summary>
public enum TrackedDevice
{
    /// <summary>The controller currently assigned to the left hand.</summary>
    LeftController,
    /// <summary>The controller currently assigned to the right hand.</summary>
    RightController,
    /// <summary>The head-mounted display.</summary>
    HMD
}
