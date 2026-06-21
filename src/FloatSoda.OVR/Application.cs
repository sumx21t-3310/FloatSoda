using FloatSoda.OVR.Exceptions;

namespace FloatSoda.OVR;

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

    Other = EVRApplicationType.VRApplication_Other
}

public class Application : IDisposable
{
    public readonly ApplicationType Type;
    public CVRSystem OVRSystem { get; init; }

    public string AppKey { get; set; }

    /// <summary>
    /// Instantiate and initialize a new <see cref="Application"/>.
    /// Internally, this will initialize the OpenVR API with the specified
    /// <paramref name="type"/> and <paramref name="startupInfo"/>.
    /// </summary>
    /// 
    /// <param name="type"></param>
    /// <param name="startupInfo"></param>
    public Application(ApplicationType type, string startupInfo = "")
    {
        Type = type;

        // Attempt to initialize a new OpenVR context
        var err = EVRInitError.None;
        OVRSystem = OpenVR.Init(ref err, (EVRApplicationType)type, startupInfo);

        err.ThrowIfError();
    }

    public void AddManifest(string path, VRManifest manifest, bool temporal = true)
    {
        var json = manifest;


        OpenVR.Applications.AddApplicationManifest(path, temporal).ThrowIfError();
    }

    public bool IsAutoLaunch
    {
        get => OpenVR.Applications.GetApplicationAutoLaunch(AppKey);
        set => OpenVR.Applications.SetApplicationAutoLaunch(AppKey, value);
    }

    public void Dispose()
    {
        OpenVR.Shutdown();
        GC.SuppressFinalize(this);
    }

    ~Application() => Dispose();
}