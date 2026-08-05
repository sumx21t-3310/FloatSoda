using FloatSoda.Abstractions.Geometries;
using FloatSoda.Core;
using FloatSoda.Elements;
using FloatSoda.RenderObjects;
using FloatSoda.RenderObjects.Painting;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;

namespace FloatSoda.Test.Widgets;

public class RepaintBoundaryTest
{
    private class PaintCounterBox : RenderProxyBox
    {
        public int PaintCount { get; private set; }

        public override void Paint(PaintingContext context, Offset offset)
        {
            PaintCount++;
            base.Paint(context, offset);
        }
    }

    [Fact]
    public void CreateRenderObject_Widgetツリーへマウント_RenderRepaintBoundaryを挟む()
    {
        var view = new RenderView(100, 100);
        _ = new RenderPipeline
        {
            OnNeedVisualUpdate = () => { },
            RenderView = view
        };
        var owner = new BuildOwner(() => { });

        _ = new RenderObjectToWidgetAdapter
        {
            Container = view,
            Child = new RepaintBoundary
            {
                Child = new SizedBox { Width = 10, Height = 10 }
            }
        }.AttachToRenderTree(owner, null);

        var boundary = Assert.IsType<RenderRepaintBoundary>(view.Child);
        Assert.True(boundary.IsRepaintBoundary);
        Assert.NotNull(boundary.Child);
    }

    [Fact]
    public void MarkNeedsPaint_境界内の子がDirty_境界内だけを再描画する()
    {
        var inside = new PaintCounterBox();
        var boundary = new RenderRepaintBoundary { Child = inside };
        var outside = new PaintCounterBox { Child = boundary };
        var view = new RenderView(100, 100) { FixedSize = new(100, 100), Child = outside };
        var pipeline = new RenderPipeline
        {
            OnNeedVisualUpdate = () => { },
            RenderView = view
        };
        view.PrepareInitialFrame();
        pipeline.FlushLayout();
        pipeline.FlushPaint();

        Assert.Equal(1, outside.PaintCount);
        Assert.Equal(1, inside.PaintCount);

        inside.MarkNeedsPaint();

        Assert.False(outside.NeedsPaint);
        Assert.False(view.NeedsPaint);
        Assert.Equal([boundary], pipeline.NodesNeedingPaint);

        pipeline.FlushPaint();

        Assert.Equal(1, outside.PaintCount);
        Assert.Equal(2, inside.PaintCount);
        Assert.False(boundary.NeedsPaint);
        Assert.Empty(pipeline.NodesNeedingPaint);
    }
}
