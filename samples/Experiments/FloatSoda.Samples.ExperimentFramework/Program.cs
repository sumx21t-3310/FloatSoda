using FloatSoda.Core;
using FloatSoda.Elements;
using FloatSoda.RenderObjects;
using FloatSoda.Samples.ExperimentFramework;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using SkiaSharp;

// 引数 no-boundary を付けると、RepaintBoundary で包まない。
var useBoundary = args.Length == 0 || args[0] != "no-boundary";

var renderView = new RenderView(400, 300);
var pipeline = new RenderPipeline { OnNeedVisualUpdate = () => { }, RenderView = renderView };
var owner = new BuildOwner(() => { });

TogglePanelState? state = null;
new RenderObjectToWidgetAdapter
{
    Container = renderView,
    Child = new TogglePanel { UseBoundary = useBoundary, OnStateCreated = s => state = s },
}.AttachToRenderTree(owner, null);

renderView.PrepareInitialFrame();
owner.BuildScope();
pipeline.FlushLayout();
pipeline.FlushPaint();
Show("初回フレーム後", pipeline);

state!.Toggle();
Show("SetState 直後", pipeline);

owner.BuildScope();
Show("BuildScope 後", pipeline);

pipeline.FlushLayout();
pipeline.FlushPaint();
Show("FlushPaint 後", pipeline);

static void Show(string label, RenderPipeline pipeline)
{
    var nodes = pipeline.NodesNeedingPaint.Select(n => $"{n.GetType().Name}@depth{n.Depth}");
    Console.WriteLine($"{label}: NodesNeedingPaint = [{string.Join(", ", nodes)}]");
}
