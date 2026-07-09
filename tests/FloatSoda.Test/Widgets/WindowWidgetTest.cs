using FloatSoda.Core;
using FloatSoda.Elements;
using FloatSoda.RenderObjects;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using SkiaSharp;

namespace FloatSoda.Test.Widgets;

public class WindowWidgetTest
{
    /// <summary>Build時に祖先のWindowWidgetを探索して記録するプローブ。</summary>
    private record Probe : StatelessWidget
    {
        public required Action<IBuildContext> OnBuild { get; init; }

        public override Widget Build(IBuildContext context)
        {
            OnBuild(context);
            return new SizedBox { Width = 10, Height = 10 };
        }
    }

    private static (RenderPipeline Pipeline, RenderView RenderView) MountTree(Widget widget)
    {
        var renderView = new RenderView();
        var pipeline = new RenderPipeline
        {
            OnNeedVisualUpdate = () => { },
            RenderView = renderView
        };
        renderView.PrepareInitialFrame();

        new RenderObjectToWidgetAdapter
        {
            Container = renderView,
            Child = widget
        }.AttachToRenderTree(new BuildOwner(() => { }), null);

        return (pipeline, renderView);
    }

    [Fact]
    public void Of_FindsConcreteWindow_ViaBaseTypeLookup()
    {
        WindowWidget? found = null;

        MountTree(new DashboardWindow
        {
            Title = "TestWindow",
            Child = new Probe { OnBuild = ctx => found = WindowWidget.Of(ctx) }
        });

        // ScopeType により具象型（DashboardWindow）でも基底型 WindowWidget で lookup できる
        Assert.IsType<DashboardWindow>(found);
        Assert.Equal("TestWindow", found!.Title);
    }

    [Fact]
    public void SizeUnset_RenderViewShrinkWrapsToChild()
    {
        var (pipeline, renderView) = MountTree(new DashboardWindow
        {
            Title = "TestWindow",
            Child = new SizedBox { Width = 320, Height = 240 }
        });

        pipeline.FlushLayout();

        Assert.Null(renderView.FixedSize);
        Assert.Equal(new SKSize(320, 240), renderView.Size);
    }

    [Fact]
    public void SizeSet_RenderViewUsesFixedSize()
    {
        var (pipeline, renderView) = MountTree(new DashboardWindow
        {
            Title = "TestWindow",
            Size = new SKSize(800, 600),
            Child = new SizedBox { Width = 320, Height = 240 }
        });

        pipeline.FlushLayout();

        Assert.Equal(new SKSize(800, 600), renderView.FixedSize);
        Assert.Equal(new SKSize(800, 600), renderView.Size);
    }
}
