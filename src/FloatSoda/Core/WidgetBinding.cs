using FloatSoda.Common.Layer;
using FloatSoda.Elements;
using FloatSoda.Engine;
using FloatSoda.OVR.Overlay;
using FloatSoda.Render;
using FloatSoda.Widgets;
using SkiaSharp;

namespace FloatSoda.Core;

public class WidgetBinding
{
    private RenderPipeline? Pipeline { get; set; }
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

    public void EnsureVisualUpdate()
    {
        NeedsVisualUpdate = true;
    }

    public void AttachRootWidget(Widget rootWidget)
    {
        var isBootStrapFrame = RenderViewElement == null;

        RenderViewElement = new RenderObjectToWidgetAdapter
            {
                Child = rootWidget,
                Container = Pipeline.RenderView
            }
            .AttachToRenderTree();

        if (isBootStrapFrame)
        {
            EnsureVisualUpdate();
        }
    }

    public void DrawFrame()
    {
        if (!NeedsVisualUpdate || Window == null) return;
        NeedsVisualUpdate = false;
        
        Pipeline?.RenderView.PrepareInitialFrame();

        Pipeline?.FlushLayout();
        Pipeline?.FlushPaint();

        if (Pipeline?.RenderView.Layer?.Clone() is not ContainerLayer layer) return;

        RenderThreadRunner?.PostRender(Window, layer);
    }
}