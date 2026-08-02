using FloatSoda.Core;
using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.RenderObjects;
using FloatSoda.RenderObjects.Layout;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using SkiaSharp;

namespace FloatSoda.Test.Widgets;

public class PaddingTest
{
    private static (
        RenderPipeline Pipeline,
        RenderView View,
        RenderObjectToWidgetElement<RenderView> Root) Mount(Widget widget)
    {
        var view = new RenderView();
        var pipeline = new RenderPipeline
        {
            OnNeedVisualUpdate = () => { },
            RenderView = view
        };
        view.PrepareInitialFrame();

        var root = new RenderObjectToWidgetAdapter
        {
            Container = view,
            Child = widget
        }.AttachToRenderTree(new BuildOwner(() => { }), null);

        return (pipeline, view, root);
    }

    [Fact]
    public void WidgetUpdate_ChangesHeadlessLayoutResult()
    {
        var initial = new Padding
        {
            Spacing = EdgeInsets.All(10),
            Child = new SizedBox { Width = 100, Height = 50 }
        };
        var (pipeline, view, root) = Mount(initial);
        pipeline.FlushLayout();
        Assert.Equal(new SKSize(120, 70), view.Size);

        new RenderObjectToWidgetAdapter
        {
            Container = view,
            Child = initial with { Spacing = EdgeInsets.All(20) }
        }.AttachToRenderTree(root.Owner!, root);
        root.Owner!.BuildScope();
        pipeline.FlushLayout();

        Assert.Equal(new SKSize(140, 90), view.Size);
        Assert.Equal(
            EdgeInsets.All(20),
            Assert.IsType<RenderPadding>(view.Child).Padding);
    }
}
