namespace FloatSoda.OVR.Exceptions;

/// <summary>Provides extension methods that translate OpenVR error values into typed exceptions.</summary>
public static class OpenVRExceptionHelper
{
    /// <summary>Throws when <paramref name="error"/> is an application error.</summary>
    public static void ThrowIfError(this EVRApplicationError error) => VRApplicationException.ThrowIfError(error);
    /// <summary>Throws when <paramref name="error"/> is an initialization error.</summary>
    public static void ThrowIfError(this EVRInitError error) => VRInitializeException.ThrowIfError(error);
    /// <summary>Throws when <paramref name="error"/> is an input error.</summary>
    public static void ThrowIfError(this EVRInputError error) => VRInputException.ThrowIfError(error);
    /// <summary>Throws when <paramref name="error"/> is an overlay error.</summary>
    public static void ThrowIfError(this EVROverlayError error) => VROverlayException.ThrowIfError(error);
    /// <summary>Throws when <paramref name="handle"/> is OpenVR's invalid overlay handle.</summary>
    public static void ThrowIfInvalidHandle(this ulong handle) => VROverlayException.ThrowIfInvalidHandle(handle);
    /// <summary>Throws when <paramref name="error"/> is a compositor error.</summary>
    public static void ThrowIfError(this EVRCompositorError error) => VRCompositorException.ThrowIfError(error);
    /// <summary>Throws when <paramref name="error"/> is a settings error.</summary>
    public static void ThrowIfError(this EVRSettingsError error) => VRSettingsException.ThrowIfError(error);
    /// <summary>Throws when <paramref name="error"/> is a notification error.</summary>
    public static void ThrowIfError(this EVRNotificationError error) => VRNotificationException.ThrowIfError(error);
    /// <summary>Throws when <paramref name="error"/> is a tracked-property error.</summary>
    public static void ThrowIfError(this ETrackedPropertyError error) => TrackedPropertyException.ThrowIfError(error);
    /// <summary>Throws when <paramref name="error"/> is a screenshot error.</summary>
    public static void ThrowIfError(this EVRScreenshotError error) => VRScreenshotException.ThrowIfError(error);
    /// <summary>Throws when <paramref name="error"/> is a render-model error.</summary>
    public static void ThrowIfError(this EVRRenderModelError error) => VRRenderModelException.ThrowIfError(error);
}
