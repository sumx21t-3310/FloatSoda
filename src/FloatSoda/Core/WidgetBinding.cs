using FloatSoda.Abstractions.Engine;
using FloatSoda.Abstractions.Geometries;
using FloatSoda.Rendering.Layers;
using FloatSoda.Elements;
using FloatSoda.Engine;
using FloatSoda.OVR.Overlay;
using FloatSoda.RenderObjects;
using FloatSoda.Widgets;
using SkiaSharp;
using OverlayWindow = FloatSoda.Engine.OverlayWindow;

namespace FloatSoda.Core;

public class WidgetBinding : IFrameScheduler
{
    public WidgetBinding()
    {
        BuildOwner = new BuildOwner(EnsureVisualUpdate) { FrameScheduler = this };
    }

    private RenderPipeline? Pipeline { get; set; }
    private BuildOwner BuildOwner { get; set; }
    public Element? RenderViewElement { get; private set; }
    private RenderThreadRunner? RenderThreadRunner { get; set; }
    private IEngineWindow? Window { get; set; }
    public bool Initialized { get; private set; }
    public bool NeedsVisualUpdate { get; private set; }

    public string? WindowName { get; private set; }

    public SKSizeI Size => Pipeline?.RenderView.Size.ToSizeI() ?? SKSizeI.Empty;

    /// <summary>直近でレンダースレッドへ通知したオーバーレイサイズ。変化検知に使用する。</summary>
    private SKSizeI _lastPostedSize = SKSizeI.Empty;

    public int NextFrameCallbackId { get; private set; } = 0;

    private Dictionary<int, Action<TimeSpan>> _transientCallbacks = [];

    private bool _hasScheduledFrame;

    public void EnsureInitialized(string windowName, RenderThreadRunner renderThreadRunner,
        Func<string, IOverlay> overlayFactory, Dpm dpm)
    {
        if (Initialized) return;
        Initialized = true;

        WindowName = windowName;

        RenderThreadRunner = renderThreadRunner;

        RenderThreadRunner.PostTask(() =>
        {
            var renderer = new Renderer
            {
                GLView = new GLView(Size)
            };

            var overlay = overlayFactory(WindowName);

            Window = new OverlayWindow(overlay, renderer, dpm);
        });


        Pipeline = new RenderPipeline
        {
            OnNeedVisualUpdate = EnsureVisualUpdate,
            RenderView = new RenderView()
        };

        Pipeline.RenderView.PrepareInitialFrame();
    }

    public void EnsureVisualUpdate() => NeedsVisualUpdate = true;

    /// <summary>
    /// このウィンドウが <see cref="DeviceTrackedOverlayTransform"/> を持つ場合、Transform を再適用します。
    /// デバイス接続/ロール変更イベント（VREvent_TrackedDeviceActivated / VREvent_TrackedDeviceRoleChanged）を
    /// 受けて FloatSodaApp から呼ばれます。未接続だった場合の初回適用に加え、
    /// 再接続で device index が変わったケースの再バインドも兼ねます（未接続なら Apply 側で安全にスキップされます）。
    /// </summary>
    public void ReapplyDeviceTrackedTransform()
    {
        if (Window is not OverlayWindow overlayWindow) return;
        if (overlayWindow.Overlay is not IMovableOverlay { Transform: DeviceTrackedOverlayTransform transform }) return;

        RenderThreadRunner?.PostTask(transform.Apply);
    }

    public void AttachRootWidget(Widget rootWidget)
    {
        var isBootStrapFrame = RenderViewElement == null;

        RenderViewElement = new RenderObjectToWidgetAdapter
            {
                Child = rootWidget,
                Container = Pipeline.RenderView
            }
            .AttachToRenderTree(BuildOwner, RenderViewElement as RenderObjectToWidgetElement<RenderView>);

        if (isBootStrapFrame)
        {
            EnsureVisualUpdate();
        }
    }

    /// <summary>
    /// ホットリロード時にWidgetツリー全体を再ビルド対象にする。
    /// 実際の再ビルドは次の<see cref="DrawFrame"/>のBuildScopeで行われる。
    /// </summary>
    public void Reassemble() => RenderViewElement?.Reassemble();

    public void DrawFrame()
    {
        if (RenderViewElement != null)
        {
            BuildOwner.BuildScope();
        }

        if (!NeedsVisualUpdate || Window == null) return;
        NeedsVisualUpdate = false;

        Pipeline?.FlushLayout();

        // レイアウト結果サイズ = オーバーレイサイズ。変化していればレンダースレッドで GLView をリサイズする。
        // 初回生成時だけでなく、MarkNeedsLayout 起点の再レイアウト（テキスト変更やホットリロード）でも
        // FlushLayout 後にここで検知されるため、同じ経路でサイズ追従できる。
        PostResizeIfSizeChanged();

        Pipeline?.FlushPaint();

        if (Pipeline?.RenderView.Layer?.Clone() is not ContainerLayer layer) return;

        RenderThreadRunner?.PostRender(Window, layer);
    }

    private void PostResizeIfSizeChanged()
    {
        var newSize = Pipeline?.RenderView.Size.ToSizeI() ?? SKSizeI.Empty;

        if (newSize == _lastPostedSize || newSize is not { Width: > 0, Height: > 0 }) return;

        _lastPostedSize = newSize;

        var window = Window;
        if (window == null) return;

        RenderThreadRunner?.PostTask(() => window.Resize(newSize));
    }

    public void BeginFrame(TimeSpan elapsedTime)
    {
        if (!Initialized) return;
        _hasScheduledFrame = false;
        var callbacks = _transientCallbacks.Values.ToList();
        _transientCallbacks.Clear();

        foreach (var callback in callbacks)
        {
            callback(elapsedTime);
        }

        DrawFrame();
    }

    public int ScheduleFrameCallback(Action<TimeSpan> callback)
    {
        ScheduleFrame();
        NextFrameCallbackId++;

        _transientCallbacks[NextFrameCallbackId] = callback;


        return NextFrameCallbackId;
    }

    public void CancelFrameCallback(int id) => _transientCallbacks.Remove(id);

    public void ScheduleFrame()
    {
        if (_hasScheduledFrame) return;

        _hasScheduledFrame = true;
    }
}
