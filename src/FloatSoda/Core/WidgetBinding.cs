using System.Diagnostics;
using FloatSoda.Abstractions.Engine;
using FloatSoda.Abstractions.Geometries;
using FloatSoda.Abstractions.Input;
using FloatSoda.Rendering.Layers;
using FloatSoda.Elements;
using FloatSoda.Engine;
using FloatSoda.Gesture;
using FloatSoda.OVR.Overlay;
using FloatSoda.RenderObjects;
using FloatSoda.Widgets;
using SkiaSharp;
using OverlayWindow = FloatSoda.Engine.OverlayWindow;

namespace FloatSoda.Core;

/// <summary>1つのウィンドウについてWidgetの再構築、レイアウト、描画、入力処理をフレーム単位で調整します。</summary>
/// <remarks>再構築またはRenderObjectのDirty通知で視覚更新を予約し、次のフレームで必要な処理だけを実行します。</remarks>
/// <seealso cref="RenderPipeline"/>
/// <seealso cref="BuildOwner"/>
public class WidgetBinding : IFrameScheduler, IHitTestTarget, IGestureBinding
{
    /// <summary>フレーム予約とジェスチャ処理をこのウィンドウへ関連付けたバインディングを初期化します。</summary>
    public WidgetBinding()
    {
        BuildOwner = new BuildOwner(EnsureVisualUpdate) { FrameScheduler = this, GestureBinding = this };
    }

    /// <inheritdoc />
    public GestureArenaManager GestureArena { get; } = new();

    /// <inheritdoc />
    public PointerRouter PointerRouter { get; } = new();

    private RenderPipeline? Pipeline { get; set; }
    private BuildOwner BuildOwner { get; set; }
    /// <summary>ウィンドウのWidgetツリーをRenderViewへ接続するルートElementを取得します。</summary>
    /// <value>ルートWidgetが接続される前は<see langword="null"/>です。</value>
    public Element? RenderViewElement { get; private set; }
    private RenderThreadRunner? RenderThreadRunner { get; set; }
    private IEngineWindow? Window { get; set; }
    /// <summary>描画ウィンドウとパイプラインの初期化が開始済みかを取得します。</summary>
    public bool Initialized { get; private set; }
    /// <summary>次のフレームでレイアウトまたは描画を処理する必要があるかを取得します。</summary>
    /// <remarks><see cref="EnsureVisualUpdate"/>で設定され、<see cref="DrawFrame"/>が更新処理を開始すると解除されます。</remarks>
    public bool NeedsVisualUpdate { get; private set; }

    /// <summary>このバインディングが管理するウィンドウ名を取得します。</summary>
    /// <value>初期化前は<see langword="null"/>です。</value>
    public string? WindowName { get; private set; }

    /// <summary>直近のレイアウトで確定したウィンドウのピクセル単位の大きさを取得します。</summary>
    /// <value>パイプラインの初期化前は空のサイズです。</value>
    public SKSizeI Size => Pipeline?.RenderView.Size.ToSizeI() ?? SKSizeI.Empty;

    /// <summary>直近でレンダースレッドへ通知したオーバーレイサイズ。変化検知に使用する。</summary>
    private SKSizeI _lastPostedSize = SKSizeI.Empty;

    /// <summary>直近に発行したフレームコールバック識別子を取得します。初期値は0です。</summary>
    public int NextFrameCallbackId { get; private set; } = 0;

    private readonly Dictionary<int, Action<TimeSpan>> _transientCallbacks = [];

    private readonly Dictionary<int, HitTestResult> _hitTests = [];

    private PointerController? _pointerController;

    private bool _hasScheduledFrame;

    /// <summary>このバインディングの描画パイプラインとエンジンウィンドウを初回だけ初期化します。</summary>
    /// <param name="windowName">作成するウィンドウの識別名。</param>
    /// <param name="renderThreadRunner">ウィンドウ作成と描画処理を実行するレンダースレッド。</param>
    /// <param name="windowFactory">初期化済みレンダラーからエンジンウィンドウを作成する処理。</param>
    /// <param name="visible">デスクトップウィンドウを作成時から表示する場合は<see langword="true"/>。既定値は<see langword="false"/>です。</param>
    /// <remarks>エンジンウィンドウの作成はレンダースレッドへ予約されるため、このメソッドから戻った時点では完了していない場合があります。</remarks>
    public void EnsureInitialized(string windowName, RenderThreadRunner renderThreadRunner,
        Func<Renderer, IEngineWindow> windowFactory, bool visible = false)
    {
        if (Initialized) return;
        Initialized = true;

        WindowName = windowName;

        RenderThreadRunner = renderThreadRunner;

        RenderThreadRunner.PostTask(() =>
        {
            var renderer = new Renderer
            {
                GLView = new GLView(Size, visible, windowName)
            };

            Window = windowFactory(renderer);
        });


        Pipeline = new RenderPipeline
        {
            OnNeedVisualUpdate = EnsureVisualUpdate,
            RenderView = new RenderView()
        };

        Pipeline.RenderView.PrepareInitialFrame();
    }

    /// <summary>次のフレームで視覚更新が必要であることを記録します。</summary>
    /// <remarks>DirtyなElementまたはRenderObjectから呼び出され、<see cref="DrawFrame"/>による再構築、レイアウト、描画の実行を予約します。</remarks>
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

    /// <summary>指定されたWidgetをこのウィンドウのRenderViewへルートとして接続します。</summary>
    /// <param name="rootWidget">接続または更新するルートWidget。</param>
    /// <remarks>初回接続では視覚更新を予約します。接続済みの場合は既存Elementを再利用して更新します。</remarks>
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

    /// <summary>予約済みのWidget再構築と視覚更新を処理し、完成したレイヤーをレンダースレッドへ送ります。</summary>
    /// <remarks>Elementの再構築は毎フレーム確認します。<see cref="NeedsVisualUpdate"/>が未設定の場合、またはウィンドウ作成前の場合はレイアウトと描画を行いません。</remarks>
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

    /// <summary>1フレーム分の入力、コールバック、および描画処理を実行します。</summary>
    /// <param name="elapsedTime">アプリケーション開始からの単調な経過時間。</param>
    /// <remarks>未初期化の場合は何も行いません。登録済みの一時コールバックはこのフレームで一度だけ実行されます。</remarks>
    public void BeginFrame(TimeSpan elapsedTime)
    {
        if (!Initialized) return;
        FlushPointerEvents();
        _hasScheduledFrame = false;
        var callbacks = _transientCallbacks.Values.ToList();
        _transientCallbacks.Clear();

        foreach (var callback in callbacks)
        {
            callback(elapsedTime);
        }

        DrawFrame();
    }

    /// <inheritdoc />
    public int ScheduleFrameCallback(Action<TimeSpan> callback)
    {
        ScheduleFrame();
        NextFrameCallbackId++;

        _transientCallbacks[NextFrameCallbackId] = callback;


        return NextFrameCallbackId;
    }

    /// <inheritdoc />
    public void CancelFrameCallback(int id) => _transientCallbacks.Remove(id);

    /// <summary>次のフレーム処理が必要であることを記録します。</summary>
    /// <remarks>同一フレーム内で複数回呼び出しても予約状態は1件にまとめられ、<see cref="BeginFrame"/>の開始時に解除されます。</remarks>
    public void ScheduleFrame()
    {
        if (_hasScheduledFrame) return;

        _hasScheduledFrame = true;
    }

    
    
    /// <summary>
    /// ウィンドウ由来の生ポインタイベントをポンプ・変換し、ヒットテストへディスパッチします。
    /// Window はレンダースレッドで非同期に生成されるため、生成が観測できたフレームから配線されます。
    /// </summary>
    private void FlushPointerEvents()
    {
        var window = Window;
        if (window == null) return;

        (window as OverlayWindow)?.PollInputEvents();

        if (_pointerController == null && window.PointerSource is { } pointerSource)
        {
            _pointerController = new PointerController(pointerSource);
            _pointerController.OnPointerEvent += HandlePointerEvent;
        }

        _pointerController?.Flush();
    }

    private void HandlePointerEvent(PointerEvent pointerEvent)
    {
        switch (pointerEvent.Phase)
        {
            case PointerEventPhase.Down:
            {
                var result = new HitTestResult();
                HitTest(result, pointerEvent.Position);
                _hitTests[pointerEvent.PointerId] = result;
                DispatchEvent(pointerEvent, result);
                break;
            }
            case PointerEventPhase.Move:
            case PointerEventPhase.Up:
            {
                // Down時のヒットパスへ届け続ける（ポインタキャプチャ）。配線前にDownが起きていた場合は捨てる。
                if (!_hitTests.TryGetValue(pointerEvent.PointerId, out var result)) return;
                DispatchEvent(pointerEvent, result);

                if (pointerEvent.Phase == PointerEventPhase.Up)
                {
                    _hitTests.Remove(pointerEvent.PointerId);
                }

                break;
            }
            case PointerEventPhase.Add:
            case PointerEventPhase.Remove:
            default:
                DispatchEvent(pointerEvent, null);
                break;
        }
    }

    private void HitTest(HitTestResult hitTestResult, Offset position)
    {
        Pipeline?.RenderView.HitTest(hitTestResult, position);
        
        hitTestResult.Add(new HitTestEntry(this));
    }
    
    /// <summary>
    /// ヒットパスを通過したポインターイベントを認識器へ配送し、フェーズに応じてアリーナを進行します。
    /// </summary>
    /// <param name="pointerEvent">認識器へ配送するポインターイベント。</param>
    /// <param name="entry">このバインディングに対応するヒットテストエントリ。</param>
    /// <remarks>
    /// Downフェーズでは認識器の参加受付を締め切り、Upフェーズでは未確定アリーナの既定勝者を確定します。
    /// </remarks>
    public void HandleEvent(PointerEvent pointerEvent, HitTestEntry entry)
    {
        // ヒットパス末尾のこのエントリで、ジェスチャ認識器へのルーティングとアリーナ調停を行う
        // (Flutter の GestureBinding.handleEvent 相当)。Down のディスパッチ中に各認識器が
        // AddPointer でルータ購読・アリーナ参加済みなので、ここでの Route が Down を届ける。
        PointerRouter.Route(pointerEvent);

        switch (pointerEvent.Phase)
        {
            case PointerEventPhase.Down:
                GestureArena.Close(pointerEvent.PointerId);
                break;
            case PointerEventPhase.Up:
                GestureArena.Sweep(pointerEvent.PointerId);
                break;
        }
    }

    private void DispatchEvent(PointerEvent pointerEvent, HitTestResult? hitTestResult)
    {
        if (hitTestResult is null)
        {
            Debug.Assert(pointerEvent.Phase is PointerEventPhase.Add or PointerEventPhase.Remove);
            return;
        }

        foreach (var entry in hitTestResult.Path)
        {
            entry.Target.HandleEvent(pointerEvent, entry);
        }
    }
}
