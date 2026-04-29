using System.Numerics;
using FloatSoda.OVR;
using FloatSoda.OVR.Exceptions;
using FloatSoda.Render;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Valve.VR;

namespace FloatSoda;

public class FloatSodaApp : IDisposable
{
    private readonly List<IWindow> _windows = [];
    private readonly List<Action> _windowBuilders = [];
    private readonly List<Action> _builders = [];

    public void CreateFloatingWindow(string key, string name, Element rootElement, float width, Vector3 position, TrackingTarget trackingTarget)
    {
        _windowBuilders.Add(() =>
        {
            var floatingWindow = new FloatingWindow(key, name, new Renderer(new GLView()), rootElement);

            floatingWindow.Transform.Position = position;
            floatingWindow.Transform.TrackingTarget = trackingTarget;
            floatingWindow.Width = width;
            _windows.Add(floatingWindow);
        });
    }

    public void CreateDashboardWindow(string key, string name, string iconPath, Element rootElement)
    {
        _windowBuilders.Add(() => { _windows.Add(new DashboardWindow(key, name, iconPath, new Renderer(new GLView()), rootElement)); });
    }

    private void AddManifestFile(string manifestPath) => _builders.Add(() => InstallManifest(manifestPath));

    public void Run(int targetFrameRate = 30)
    {
        if (!GLFW.Init()) throw new Exception();
        GLFW.Init();
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