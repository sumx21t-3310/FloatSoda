using System.Text.RegularExpressions;
using FloatSoda.OVR.Exceptions;

namespace FloatSoda.OVR;

/// <summary>Specifies the capabilities and startup behavior requested from OpenVR.</summary>
public enum ApplicationType
{
    /// <summary>
    /// A 3D application that will be drawing an environment.
    /// </summary>
    Scene = EVRApplicationType.VRApplication_Scene,

    /// <summary>
    /// An application that only interacts with overlays or the dashboard.
    /// </summary>
    Overlay = EVRApplicationType.VRApplication_Overlay,

    /// <summary>
    /// The application will not start SteamVR. If it is not already running
    /// the call with VR_Init will fail with <see cref="EVRInitError.Init_NoServerForBackgroundApp"/>.
    /// </summary>
    Background = EVRApplicationType.VRApplication_Background,

    /// <summary>
    /// The application will start up even if no hardware is present. Only the IVRSettings
    /// and IVRApplications interfaces are guaranteed to work. This application type is
    /// appropriate for things like installers.
    /// </summary>
    Utility = EVRApplicationType.VRApplication_Utility,

    /// <summary>An application that does not fit one of the specialized OpenVR categories.</summary>
    Other = EVRApplicationType.VRApplication_Other
}

/// <summary>
/// A validated SteamVR application key. Restricted to ASCII lowercase letters, digits,
/// dots, and underscores, and must not exceed 127 characters — OpenVR's
/// <c>k_unMaxApplicationKeyLength</c> is 128, including the null terminator.
/// </summary>
public readonly record struct AppKey(string Value)
{
    /// <summary>Gets the validated application key.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The value contains more than 127 characters.</exception>
    /// <exception cref="FormatException">The value is empty or contains an unsupported character.</exception>
    public string Value
    {
        get;
        init
        {
            if (value.Length > 127) throw new ArgumentOutOfRangeException(nameof(value));
            if (!Regex.IsMatch(value, "^[a-z0-9_.]+$")) throw new FormatException();

            field = value;
        }
    } = Value;

    /// <summary>Returns the application key.</summary>
    public override string ToString() => Value;

    /// <summary>Returns the string value of a validated application key.</summary>
    /// <param name="key">The application key to convert.</param>
    public static implicit operator string(AppKey key) => key.Value;
}

/// <summary>Describes the identity and OpenVR application type used to initialize an application.</summary>
/// <param name="Key">The application's registered SteamVR key.</param>
/// <param name="Type">The capabilities and startup behavior requested from OpenVR.</param>
public record OVRAppInfo(AppKey Key, ApplicationType Type = ApplicationType.Overlay)
{
    /// <summary>Gets startup information passed to OpenVR. Currently always empty.</summary>
    public string StartupInfo => "";
}

/// <summary>Owns an initialized OpenVR application context.</summary>
/// <remarks>Dispose the instance after all OpenVR objects have stopped using the process-wide context.</remarks>
public class OVRApplication : IDisposable
{
    /// <summary>Gets the initialized OpenVR system interface.</summary>
    public CVRSystem OVRSystem { get; init; }
    /// <summary>Gets the application information used for initialization.</summary>
    public OVRAppInfo Info { get; init; }

    /// <summary>
    /// Instantiate and initialize a new <see cref="OVRApplication"/>.
    /// Internally, this initializes the process-wide OpenVR API using <paramref name="appInfo"/>.
    /// </summary>
    /// <param name="appInfo">The application identity and type used to initialize OpenVR.</param>
    /// <exception cref="VRInitializeException">OpenVR initialization fails.</exception>
    public OVRApplication(OVRAppInfo appInfo)
    {
        Info = appInfo;
        // Attempt to initialize a new OpenVR context
        var err = EVRInitError.None;
        OVRSystem = OpenVR.Init(ref err, (EVRApplicationType)Info.Type, Info.StartupInfo);

        err.ThrowIfError();
    }

    /// <summary>Gets or sets whether SteamVR launches this application automatically.</summary>
    public bool AutoLaunch
    {
        get => OpenVR.Applications.GetApplicationAutoLaunch(Info.Key);
        set => OpenVR.Applications.SetApplicationAutoLaunch(Info.Key, value).ThrowIfError();
    }


    /// <summary>Gets whether the application key is currently registered with SteamVR.</summary>
    public bool Installed => OpenVR.Applications.IsApplicationInstalled(Info.Key);

    /// <summary>
    /// Registers this process with SteamVR under <see cref="Info"/>'s application key.
    /// </summary>
    /// <remarks>
    /// <see cref="OpenVR.Init"/> only bootstraps the OpenVR API; it does not associate the
    /// calling process with a manifest's <c>app_key</c>. Call this once during startup — SteamVR
    /// needs the process/AppKey association for <see cref="AutoLaunch"/> and scene-transition
    /// behavior to work correctly. Requires <see cref="Info"/>'s key to already be registered
    /// with SteamVR (see <see cref="OVRApplications.AddManifest"/> and <see cref="Installed"/>);
    /// otherwise this throws <see cref="Exceptions.VRApplicationException"/> with
    /// <see cref="EVRApplicationError.UnknownApplication"/>.
    /// </remarks>
    /// <exception cref="Exceptions.VRApplicationException">The application key is not registered with SteamVR, or another OpenVR application error occurred.</exception>
    public void Identify() =>
        OpenVR.Applications.IdentifyApplication((uint)Environment.ProcessId, Info.Key).ThrowIfError();

    /// <summary>Shuts down the process-wide OpenVR context.</summary>
    public void Dispose()
    {
        OpenVR.Shutdown();
        GC.SuppressFinalize(this);
    }

    /// <summary>Releases the OpenVR context if the application was not disposed explicitly.</summary>
    ~OVRApplication() => Dispose();
}

/// <summary>
/// Stateless utility operations that target SteamVR's application registry as a whole,
/// rather than the calling process's own <see cref="OVRApplication"/> identity.
/// Requires <see cref="OpenVR.Init"/> to have already been called (via <see cref="OVRApplication"/>).
/// </summary>
public static class OVRApplications
{
    /// <summary>Launches the application registered with the given <paramref name="appKey"/>.</summary>
    /// <param name="appKey">The registered SteamVR application key.</param>
    public static void Launch(string appKey) => OpenVR.Applications.LaunchApplication(appKey).ThrowIfError();

    /// <summary>Adds an application manifest to SteamVR.</summary>
    /// <param name="path">The absolute path to the manifest file.</param>
    /// <param name="temporal">Whether the registration lasts only for the current SteamVR session.</param>
    public static void AddManifest(string path, bool temporal = false)
    {
        OpenVR.Applications.AddApplicationManifest(path, temporal).ThrowIfError();
    }

    /// <summary>Removes an application manifest from SteamVR.</summary>
    /// <param name="path">The absolute path to the manifest file.</param>
    public static void RemoveApplicationManifest(string path)
    {
        OpenVR.Applications.RemoveApplicationManifest(path).ThrowIfError();
    }

    /// <summary>Checks whether SteamVR can launch the registered application.</summary>
    /// <param name="appKey">The registered SteamVR application key.</param>
    public static void PerformPreLaunchCheck(string appKey)
    {
        OpenVR.Applications.PerformApplicationPrelaunchCheck(appKey).ThrowIfError();
    }
}
