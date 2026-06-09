namespace FloatSoda.OVR.Exceptions;

public static class OpenVRExceptionHelper
{
    public static void ThrowIfError(this EVRApplicationError error) => VRApplicationException.ThrowIfError(error);
    public static void ThrowIfError(this EVRInitError error) => VRInitializeException.ThrowIfError(error);
    public static void ThrowIfError(this EVRInputError error) => VRInputException.ThrowIfError(error);
    public static void ThrowIfError(this EVROverlayError error) => VROverlayException.ThrowIfError(error);
    public static void ThrowIfInvalidHandle(this ulong handle) => VROverlayException.ThrowIfInvalidHandle(handle);
    public static void ThrowIfError(this EVRCompositorError error) => VRCompositorException.ThrowIfError(error);
    public static void ThrowIfError(this EVRSettingsError error) => VRSettingsException.ThrowIfError(error);
    public static void ThrowIfError(this EVRNotificationError error) => VRNotificationException.ThrowIfError(error);
    public static void ThrowIfError(this ETrackedPropertyError error) => TrackedPropertyException.ThrowIfError(error);
    public static void ThrowIfError(this EVRScreenshotError error) => VRScreenshotException.ThrowIfError(error);
    public static void ThrowIfError(this EVRRenderModelError error) => VRRenderModelException.ThrowIfError(error);
}