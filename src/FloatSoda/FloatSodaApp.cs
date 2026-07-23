using FloatSoda.OVR;
using FloatSoda.OVR.Exceptions;
using System.Collections.Concurrent;
using FloatSoda.Abstractions.Scheduling;
using FloatSoda.Core;
using FloatSoda.Engine;
using FloatSoda.OVR.Input;
using FloatSoda.OVR.Overlay;
using FloatSoda.Widgets;
using Microsoft.Extensions.Logging;
using OverlayWindow = FloatSoda.Widgets.OverlayWindow;
using DesktopWindow = FloatSoda.Widgets.DesktopWindow;
using EngineOverlayWindow = FloatSoda.Engine.OverlayWindow;
using EngineDesktopWindow = FloatSoda.Engine.DesktopWindow;

namespace FloatSoda;

/// <summary>OpenVRのイベント処理と各ウィンドウのフレーム処理を実行するFloatSodaアプリケーションです。</summary>
/// <remarks>作成したウィンドウはメインループで初期化され、描画処理は専用のレンダースレッドへ送られます。</remarks>
/// <seealso cref="WidgetBinding"/>
public class FloatSodaApp : IDisposable
{
    private readonly RenderThreadRunner _renderThreadRunner;

    private readonly ILogger? _logger;
    private readonly ConcurrentDictionary<string, WidgetBinding> _bindings = [];

    private OVRApplication? _openVR;
    private VRSystemEventDispatcher? _dispatcher;
    private VRInputUpdater? _inputUpdater;
    private readonly IReadOnlyList<InputActionMap> _inputActionMaps;

    private readonly CancellationTokenSource _cts = new();
    private readonly IFramePacer _mainFramePacer;

    private readonly OVRAppInfo _appInfo;

    /// <summary>フレームタイムスタンプの供給元。テストではFakeTimeProvider等に差し替え可能。</summary>
    private readonly TimeProvider _timeProvider;

    /// <summary>経過時間のゼロ点(アプリ生成時)。壁時計ではなく単調タイムスタンプを使う。</summary>
    private readonly long _startTimestamp;

    private bool _disposed;
    private readonly ConcurrentQueue<Action> _pendingTasks = new();

    private static readonly ConcurrentDictionary<FloatSodaApp, byte> ActiveApps = new();

    internal FloatSodaApp(IFramePacer mainFramePacer,
        IFramePacer renderFramePacer,
        OVRAppInfo appInfo, ILoggerFactory? loggerFactory = null,
        TimeProvider? timeProvider = null,
        IReadOnlyList<InputActionMap>? inputActionMaps = null)
    {
        _mainFramePacer = mainFramePacer;
        _appInfo = appInfo;
        _inputActionMaps = inputActionMaps ?? [];
        _timeProvider = timeProvider ?? TimeProvider.System;
        _startTimestamp = _timeProvider.GetTimestamp();
        _renderThreadRunner =
            new RenderThreadRunner("RenderThread", renderFramePacer,
                loggerFactory?.CreateLogger<RenderThreadRunner>());
        _logger = loggerFactory?.CreateLogger<FloatSodaApp>();
        ActiveApps.TryAdd(this, 0);
    }

    /// <summary>
    /// ホットリロード（MetadataUpdateHandler）から任意スレッドで呼ばれる。
    /// 各アプリのメインループにReassembleを積み、次フレームで全Widgetツリーを再ビルドさせる。
    /// </summary>
    internal static void ScheduleReassembleAll()
    {
        foreach (var app in ActiveApps.Keys)
        {
            app._pendingTasks.Enqueue(app.Reassemble);
        }
    }

    private void Reassemble()
    {
        foreach (var (_, binding) in _bindings)
        {
            binding.Reassemble();
        }

        _logger?.LogInformation("ホットリロード: 全Widgetツリーを再ビルドします");
    }

    /// <summary>
    /// ウィンドウ定義 <see cref="WindowWidget"/> からウィンドウを作成します。
    /// <see cref="OverlayWindow"/> 系はOpenVRオーバーレイとして、<see cref="DesktopWindow"/> は
    /// デスクトップ上の可視ウィンドウとして表示されます。<see cref="WindowWidget"/> はウィジェットツリーの
    /// ルートとしてマウントされ、<see cref="WindowWidget.Size"/> が null の場合、ウィンドウのサイズは
    /// <see cref="InheritedWidget.Child"/> のレイアウト結果に追従します。
    /// </summary>
    public void CreateWindow(WindowWidget window)
    {
        Action<WidgetBinding> initialize = window switch
        {
            OverlayWindow overlayWindow => binding => binding.EnsureInitialized(window.Title, _renderThreadRunner,
                renderer => new EngineOverlayWindow(overlayWindow.CreateOverlay(), renderer, overlayWindow.Dpm)),
            DesktopWindow => binding => binding.EnsureInitialized(window.Title, _renderThreadRunner,
                renderer => new EngineDesktopWindow(renderer), visible: true),
            _ => throw new NotSupportedException($"{window.GetType().Name} は未対応のウィンドウ種別です。")
        };

        _pendingTasks.Enqueue(() =>
        {
            var title = window.Title;
            var widgetBinding = new WidgetBinding();
            _bindings.TryAdd(title, widgetBinding);
            initialize(widgetBinding);
            widgetBinding.AttachRootWidget(window);
            _logger?.LogInformation("{Title}を作成しました", title);
        });
    }


    /// <summary>アプリケーションを初期化し、停止要求を受けるまでメインループを実行します。</summary>
    /// <remarks>メインループ終了時には、このインスタンスが保持するOpenVRおよびレンダースレッドのリソースを解放します。</remarks>
    [STAThread]
    public void Run()
        => Run(CancellationToken.None, static () => { });

    internal void Run(CancellationToken cancellationToken, Action started)
    {
        using var cancellationRegistration = cancellationToken.Register(RequestStop);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            Initialize();
            started();

            MainLoop();
        }
        finally
        {
            Dispose();
        }
    }

    internal void RequestStop()
    {
        try
        {
            _cts.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // すでに終了処理が完了している場合、追加の停止要求は何もしない。
        }
    }


    private void MainLoop()
    {
        while (!_cts.Token.IsCancellationRequested)
        {
            try
            {
                while (_pendingTasks.TryDequeue(out var task))
                {
                    task();
                }
            }
            catch (Exception e)
            {
                _logger?.LogError("タスクエラー: {Exception}", e);
                break;
            }

            try
            {
                PollEvents();
                _inputUpdater?.Update();
            }
            catch (Exception e)
            {
                _logger?.LogError("イベント処理エラー: {Exception}", e);
                break;
            }

            try
            {
                DrawFrame();
            }
            catch (Exception e)
            {
                _logger?.LogError("描画エラー: {Exception}", e);
                break;
            }

            _mainFramePacer.WaitForNextFrame(_cts.Token);
        }
    }

    private void Initialize()
    {
        try
        {
            _openVR = new OVRApplication(_appInfo);

            try
            {
                _openVR.Identify();
            }
            catch (VRApplicationException e)
            {
                // マニフェスト未登録のAppKeyでも起動できるように、識別失敗は致命的エラーにしない
                // (AutoLaunch/シーン遷移などマニフェスト連携機能のみが動作しなくなる)。
                _logger?.LogWarning("SteamVRへのアプリケーション識別に失敗しました: {Message}", e.Message);
            }

            _dispatcher = new VRSystemEventDispatcher();

            _dispatcher.Register(EVREventType.VREvent_Quit, (in _) =>
            {
                _openVR?.OVRSystem.AcknowledgeQuit_Exiting();
                _cts.Cancel();
            });

            _dispatcher.Register(EVREventType.VREvent_ProcessQuit, (in _) => _cts.Cancel());

            // 起動時にコントローラーが未接続だったDeviceTrackedWindowへ、接続/ロール確定を機に再適用する。
            _dispatcher.Register(EVREventType.VREvent_TrackedDeviceActivated,
                (in _) => ReapplyPendingDeviceTrackedTransforms());
            
            _dispatcher.Register(EVREventType.VREvent_TrackedDeviceRoleChanged,
                (in _) => ReapplyPendingDeviceTrackedTransforms());

            if (_inputActionMaps.Count > 0)
            {
                try
                {
                    var manifestPath = ActionManifestWriter.Write(_appInfo.Key, _inputActionMaps);
                    _inputUpdater = new VRInputUpdater(manifestPath, _inputActionMaps);
                }
                catch (VRInputException e)
                {
                    // 入力が使えなくてもオーバーレイの表示は継続できるため、致命的エラーにしない。
                    _logger?.LogWarning("アクション入力の初期化に失敗しました: {Message}", e.Message);
                }
            }

            _renderThreadRunner.Start(_cts.Token);
        }
        catch (OpenVRSystemException<EVRInitError> e)
        {
            _logger?.LogError("OpenVRの初期化に失敗しました: {Message}", e.Message);
            throw;
        }
        catch (Exception e)
        {
            _logger?.LogError("致命的な起動エラー: {Message}", e.Message);
            throw;
        }
    }

    private void PollEvents() => _dispatcher?.PollEvents();

    private void ReapplyPendingDeviceTrackedTransforms()
    {
        foreach (var (_, binding) in _bindings)
        {
            binding.ReapplyDeviceTrackedTransform();
        }
    }


    private void DrawFrame()
    {
        // 1フレーム1回だけサンプリングし、全ウィンドウに同一のタイムスタンプを配る
        var elapsed = _timeProvider.GetElapsedTime(_startTimestamp);

        foreach (var (_, binding) in _bindings)
        {
            binding.BeginFrame(elapsed);
        }
    }

    /// <summary>メインループへ停止を要求し、レンダースレッドとOpenVRリソースを解放します。</summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed) return;

        ActiveApps.TryRemove(this, out _);

        if (disposing)
        {
            _cts.Cancel();

            _renderThreadRunner.Stop();

            _cts.Dispose();
        }

        _openVR?.Dispose();
        _openVR = null;

        _disposed = true;
    }
}
