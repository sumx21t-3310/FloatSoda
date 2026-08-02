using FloatSoda.Abstractions.Geometries;
using FloatSoda.Core;
using FloatSoda.Geometrics;
using FloatSoda.Gesture;
using FloatSoda.RenderObjects;
using FloatSoda.RenderObjects.Gesture;
using FloatSoda.RenderObjects.Layout;
using FloatSoda.RenderObjects.Painting;
using SkiaSharp;

namespace FloatSoda.Test.RenderObjects;

public class RenderPaddingTest
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

    private static RenderPointerListener BuildListener(double width = 100, double height = 50) => new()
    {
        Child = new RenderColoredBox
        {
            Child = new RenderConstrainedBox
            {
                AdditionalConstraints = BoxConstraints.Tight(width, height)
            }
        }
    };

    [Fact]
    public void PerformLayout_AddsPaddingAndOffsetsChild()
    {
        var child = new RenderConstrainedBox
        {
            AdditionalConstraints = BoxConstraints.Tight(100, 50)
        };
        var padding = new RenderPadding
        {
            Padding = new EdgeInsets(10, 20, 30, 40),
            Child = child
        };
        var (view, pipeline) = Build(padding);

        pipeline.FlushLayout();

        Assert.Equal(new SKSize(140, 110), padding.Size);
        Assert.Equal(padding.Size, view.Size);
        Assert.Equal(new Offset(10, 20), Assert.IsType<BoxParentData>(child.ParentData).Offset);
    }

    [Fact]
    public void PerformLayout_DeflatesTightConstraintsForChild()
    {
        var child = new RenderConstrainedBox
        {
            AdditionalConstraints = BoxConstraints.Tight(200, 200)
        };
        var padding = new RenderPadding
        {
            Padding = EdgeInsets.All(10),
            Child = child
        };

        padding.Layout(BoxConstraints.Tight(120, 100));

        Assert.Equal(new SKSize(100, 80), child.Size);
        Assert.Equal(new SKSize(120, 100), padding.Size);
    }

    [Fact]
    public void PaddingChange_MarksLayoutDirtyAndUpdatesSize()
    {
        var padding = new RenderPadding
        {
            Padding = EdgeInsets.All(10),
            Child = new RenderConstrainedBox
            {
                AdditionalConstraints = BoxConstraints.Tight(100, 50)
            }
        };
        var (view, pipeline) = Build(padding);
        pipeline.FlushLayout();

        padding.Padding = EdgeInsets.All(20);
        pipeline.FlushLayout();

        Assert.Equal(new SKSize(140, 90), padding.Size);
        Assert.Equal(padding.Size, view.Size);
    }

    [Fact]
    public void HitTestChildren_UsesPaddingOffset()
    {
        var listener = BuildListener();
        var padding = new RenderPadding
        {
            Padding = new EdgeInsets(20, 30, 0, 0),
            Child = listener
        };
        var (view, pipeline) = Build(padding);
        pipeline.FlushLayout();

        var inside = new HitTestResult();
        view.HitTest(inside, new Offset(25, 35));

        var outside = new HitTestResult();
        view.HitTest(outside, new Offset(5, 5));

        Assert.Contains(inside.Path, entry => ReferenceEquals(entry.Target, listener));
        Assert.DoesNotContain(outside.Path, entry => ReferenceEquals(entry.Target, listener));
    }

    [Fact]
    public void Padding_RejectsNegativeValues()
    {
        var padding = new RenderPadding();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => padding.Padding = new EdgeInsets(0, -1, 0, 0));
    }
}
