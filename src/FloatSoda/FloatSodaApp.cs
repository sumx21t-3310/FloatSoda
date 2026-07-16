using FloatSoda.OVR;
using FloatSoda.OVR.Exceptions;
using System.Collections.Concurrent;
using FloatSoda.Abstractions.Scheduling;
using FloatSoda.Core;
using FloatSoda.Engine;
using FloatSoda.OVR.Overlay;
using FloatSoda.Widgets;
using Microsoft.Extensions.Logging;
using OverlayWindow = FloatSoda.Widgets.OverlayWindow;

namespace FloatSoda;

public class FloatSodaApp : IDisposable
{
    private readonly RenderThreadRunner _renderThreadRunner;

    private readonly ILogger? _logger;
    private readonly ConcurrentDictionary<string, WidgetBinding> _bindings = [];

    private OVRApplication? _openVR;
    private VRSystemEventDispatcher? _dispatcher;

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
        TimeProvider? timeProvider = null)
    {
        _mainFramePacer = mainFramePacer;
        _appInfo = appInfo;
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
    /// ウィンドウ定義 <see cref="WindowWidget"/> からオーバーレイウィンドウを作成します。
    /// <see cref="WindowWidget"/> はウィジェットツリーのルートとしてマウントされ、
    /// <see cref="WindowWidget.Size"/> が null の場合、オーバーレイのサイズは
    /// <see cref="InheritedWidget.Child"/> のレイアウト結果に追従します。
    /// </summary>
    public void CreateWindow(WindowWidget window)
    {
        // 現状はOpenVRオーバーレイのみ対応。デスクトップウィンドウ等は今後の派生で追加する。
        if (window is not OverlayWindow overlayWindow)
        {
            throw new NotSupportedException($"{window.GetType().Name} は未対応のウィンドウ種別です。");
        }

        _pendingTasks.Enqueue(() =>
        {
            var title = window.Title;
            var widgetBinding = new WidgetBinding();
            _bindings.TryAdd(title, widgetBinding);
            widgetBinding.EnsureInitialized(title, _renderThreadRunner, _ => overlayWindow.CreateOverlay(),
                overlayWindow.Dpm);
            widgetBinding.AttachRootWidget(window);
            _logger?.LogInformation("{Title}を作成しました", title);
        });
    }


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
