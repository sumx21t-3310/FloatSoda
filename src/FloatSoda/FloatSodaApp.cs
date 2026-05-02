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
    private bool _disposed;

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
        try
        {
            try
            {
                if (!GLFW.Init()) throw new Exception("GLFWの初期化に失敗しました。");
                InitializeOpenVR();

                foreach (var build in _builders) build();
                foreach (var build in _windowBuilders) build();
            }
            catch (EVRDriverEVRInitializeException e)
            {
                Console.WriteLine($"OpenVRのドライバーの初期化に失敗しました: {e.Message}");
                return; // throwせずにreturnすることで、直ちにfinally(Dispose)へ移行
            }
            catch (EVRInitializeException e)
            {
                Console.WriteLine($"OpenVRの初期化に失敗しました: {e.Message}");
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine($"致命的な起動エラー: {e.Message}");
                return;
            }

            // --- 2. メインループセクション ---
            var limiter = new FrameLimiter(targetFrameRate);

            while (!_disposed)
            {
                try
                {
                    foreach (var window in _windows)
                    {
                        window.Update();
                    }

                    limiter.Wait();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"ループ実行中にエラーが発生しました: {e}");
                    return;
                }
            }
        }
        finally
        {
            Dispose();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;

        foreach (var window in _windows)
        {
            window.Dispose();
        }

        if (OpenVR.System != null)
        {
            OpenVR.Shutdown();
        }

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
}