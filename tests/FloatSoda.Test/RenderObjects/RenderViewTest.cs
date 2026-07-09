using FloatSoda.Core;
using FloatSoda.Geometrics;
using FloatSoda.RenderObjects;
using FloatSoda.RenderObjects.Layout;
using SkiaSharp;

namespace FloatSoda.Test.RenderObjects;

public class RenderViewTest
{
    private static (RenderView View, RenderPipeline Pipeline) Build(RenderBox child)
    {
        var view = new RenderView { Child = child };
        var pipeline = new RenderPipeline
        {
            OnNeedVisualUpdate = () => { },
            RenderView = view
        };
        view.PrepareInitialFrame();
        return (view, pipeline);
    }

    [Fact]
    public void Size_ShrinkWrapsToChild()
    {
        // Loosen(unbounded)制約により、RenderView.Size は子のレイアウト結果に収縮する。
        var (view, pipeline) = Build(new RenderConstrainedBox
        {
            AdditionalConstraints = BoxConstraints.Tight(320, 240)
        });

        pipeline.FlushLayout();

        Assert.Equal(new SKSize(320, 240), view.Size);
    }

    [Fact]
    public void Size_FollowsChildSizeChange_ViaMarkNeedsLayout()
    {
        var box = new RenderConstrainedBox
        {
            AdditionalConstraints = BoxConstraints.Tight(100, 100)
        };
        var (view, pipeline) = Build(box);

        pipeline.FlushLayout();
        Assert.Equal(new SKSize(100, 100), view.Size);

        // 子のサイズが変わると MarkNeedsLayout が RenderView まで伝播し、Size が追従する。
        box.AdditionalConstraints = BoxConstraints.Tight(200, 150);
        pipeline.FlushLayout();

        Assert.Equal(new SKSize(200, 150), view.Size);
    }

    [Fact]
    public void Size_IsEmpty_WhenNoChild()
    {
        var (view, pipeline) = Build(child: null!);

        pipeline.FlushLayout();

        Assert.True(view.Size.IsEmpty);
    }
}
