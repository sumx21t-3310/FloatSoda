using System.Numerics;
using FloatSoda.Engine;
using FloatSoda.Engine.OVR;
using FloatSoda.Engine.OVR.Exceptions;
using FloatSoda.Engine.Render;
using FloatSoda.Engine.Painting;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Valve.VR;

namespace FloatSoda;

public class FloatSodaApp : IDisposable
{
    private string _overlayKey = SteamVRKeyUtil.CreateKeyFromAssembly();

    private readonly List<IWindow> _windows = [];
    private readonly List<Action> _windowBuilders = [];
    private readonly List<Action> _builders = [];

    public void CreateFloatingWindow(string windowName, ILayer root, float width = 0.5f, Vector3? position = null, Quaternion? rotation = null, TrackingTarget trackingTarget = TrackingTarget.World)
    {
        _windowBuilders.Add(() =>
        {
            var uniqueKey = SteamVRKeyUtil.CreateWindowKey(_overlayKey, windowName);


            var floatingWindow = new FloatingWindow(uniqueKey, $"{_overlayKey}.{windowName}", new Renderer(new GLView()), root);

            floatingWindow.Transform.Position = position ?? new Vector3(0, 0, 0);
            floatingWindow.Transform.Rotation = rotation ?? Quaternion.Identity;
            floatingWindow.Transform.TrackingTarget = trackingTarget;
            floatingWindow.Width = width;
            _windows.Add(floatingWindow);
        });
    }


    public void CreateDashboardWindow(string windowName, string iconPath, ILayer root)
    {
        _windowBuilders.Add(() =>
        {
            var uniqueKey = SteamVRKeyUtil.CreateWindowKey(_overlayKey, windowName);
            _windows.Add(new DashboardWindow(uniqueKey, windowName, iconPath, new Renderer(new GLView()), root));
        });
    }

    public void SetCustomOverlayKey(string overlayKey) => _builders.Add(() => _overlayKey = overlayKey);
    private void AddManifestFile(string manifestPath) => _builders.Add(() => InstallManifest(manifestPath));

    [STAThread]
    public void Run(int targetFrameRate = 30)
    {
        if (!GLFW.Init()) throw new Exception();
        InitializeOpenVR();

        foreach (var build in _builders) build();

        foreach (var build in _windowBuilders) build();

        var limiter = new FrameLimiter(targetFrameRate);

        while (true)
        {
            foreach (var window in _windows)
            {
                window.Update();
            }

            limiter.Wait();
        }
    }

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Dispose();
        }

        Shutdown();
        GLFW.Terminate();
    }

    private void InitializeOpenVR()
    {
        var error = EVRInitError.None;
        OpenVR.Init(ref error, EVRApplicationType.VRApplication_Overlay);
        error.ThrowIfError();
    }

    private void InstallManifest(string manifestPath)
    {
        OpenVR.Applications.RemoveApplicationManifest(manifestPath).ThrowIfError();
        OpenVR.Applications.AddApplicationManifest(manifestPath, false).ThrowIfError();
    }


    private void Shutdown()
    {
        if (OpenVR.System == null) return;
        OpenVR.Shutdown();
    }
}