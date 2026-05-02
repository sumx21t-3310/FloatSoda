using System.Numerics;
using System.Runtime.InteropServices;
using FloatSoda.Engine;
using FloatSoda.Engine.OVR;
using FloatSoda.Engine.OVR.Exceptions;
using FloatSoda.Engine.Painting;
using FloatSoda.Engine.Tread;
using Valve.VR;

namespace FloatSoda;

public class FloatSodaApp : IDisposable
{
    private string _overlayKey = SteamVRKeyFactory.CreateKeyFromAssembly();

    private readonly List<Action> _builders = [];
    private readonly RenderThreadRunner _renderThreadRunner = new("RenderThread", 60);

    private readonly CancellationTokenSource _cts = new();
    private bool _disposed;

    public void CreateFloatingWindow(string windowName, ILayer root, float width = 0.5f, Vector3? position = null, Quaternion? rotation = null, TrackingTarget trackingTarget = TrackingTarget.World)
    {
        var uniqueKey = SteamVRKeyFactory.CreateWindowKey(_overlayKey, windowName);
        _renderThreadRunner.CreateFloatingWindow(uniqueKey, windowName, root, width, position, rotation, trackingTarget);
    }


    public void CreateDashboardWindow(string windowName, string iconPath, ILayer root)
    {
        var uniqueKey = SteamVRKeyFactory.CreateWindowKey(_overlayKey, windowName);
        _renderThreadRunner.CreateDashboardWindow(uniqueKey, windowName, iconPath, root);
    }

    public void SetCustomOverlayKey(string overlayKey) => _builders.Add(() => _overlayKey = overlayKey);
    private void AddManifestFile(string manifestPath) => _builders.Add(() => InstallManifest(manifestPath));

    [STAThread]
    public void Run(int targetFrameRate = 60)
    {
        try
        {
            try
            {
                InitializeOpenVR();

                foreach (var build in _builders) build();

                _renderThreadRunner.Start(_cts.Token);
            }
            catch (EVRDriverEVRInitializeException e)
            {
                Console.WriteLine($"OpenVRのドライバーの初期化に失敗しました: {e.Message}");
                return;
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

            while (!_disposed && !_cts.Token.IsCancellationRequested)
            {
                try
                {
                    VREvent_t vrEvent = default;

                    while (OpenVR.System?.PollNextEvent(ref vrEvent, (uint)Marshal.SizeOf<VREvent_t>()) ?? false)
                    {
                        var eventType = (EVREventType)vrEvent.eventType;

                        if (eventType is EVREventType.VREvent_Quit or EVREventType.VREvent_ProcessQuit)
                        {
                            OpenVR.System.AcknowledgeQuit_Exiting();

                            _cts.Cancel();
                        }
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

        _cts.Cancel();

        while (_renderThreadRunner.IsRunning)
        {
            Thread.Sleep(10);
        }

        if (OpenVR.System != null)
        {
            OpenVR.Shutdown();
        }
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