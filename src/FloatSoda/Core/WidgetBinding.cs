using FloatSoda.Common.Layer;
using FloatSoda.Elements;
using FloatSoda.Engine;
using FloatSoda.OVR.Overlay;
using FloatSoda.RenderObjects;
using FloatSoda.Widgets;
using SkiaSharp;

namespace FloatSoda.Core;

public class WidgetBinding
{
    public WidgetBinding()
    {
        BuildOwner = new BuildOwner(EnsureVisualUpdate);
    }

    private RenderPipeline? Pipeline { get; set; }
    private BuildOwner BuildOwner { get; set; }
    public Element? RenderViewElement { get; private set; }
    private RenderThreadRunner? RenderThreadRunner { get; set; }
    private IWindow? Window { get; set; }
    public bool Initialized { get; private set; }
    public bool NeedsVisualUpdate { get; private set; }

    public string? WindowName { get; private set; }

    public SKSizeI Size => Pipeline?.RenderView.Size.ToSizeI() ?? SKSizeI.Empty;


    public void EnsureInitialized(string windowName, SKSize size, RenderThreadRunner renderThreadRunner,
        Func<string, IOverlay> overlayFactory)
    {
        if (Initialized) return;
        Initialized = true;

        WindowName = windowName;

        RenderThreadRunner = renderThreadRunner;

        RenderThreadRunner.PostTask(() =>
        {
            var renderer = new Renderer
            {
                GLView = new GLView
                {
                    Size = Size
                }
            };

            var overlay = overlayFactory(WindowName);

            Window = new OverlayWindow(overlay, renderer);
        });


        Pipeline = new RenderPipeline
        {
            OnNeedVisualUpdate = EnsureVisualUpdate,
            RenderView = new RenderView(size.Width, size.Height)
        };

        Pipeline.RenderView.PrepareInitialFrame();
    }

    public void EnsureVisualUpdate() => NeedsVisualUpdate = true;

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
        Pipeline?.FlushPaint();

        if (Pipeline?.RenderView.Layer?.Clone() is not ContainerLayer layer) return;

        RenderThreadRunner?.PostRender(Window, layer);
    }
}