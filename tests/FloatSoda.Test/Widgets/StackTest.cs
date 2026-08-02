using FloatSoda.Abstractions.Geometries;
using FloatSoda.Core;
using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.RenderObjects;
using FloatSoda.RenderObjects.Layout;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;

namespace FloatSoda.Test.Widgets;

public class StackTest
{
    [Fact]
    public void WidgetUpdate_Positioned変更後にレイアウト結果を更新する()
    {
        var owner = new BuildOwner(() => { });
        var view = new RenderView();
        var pipeline = new RenderPipeline
        {
            OnNeedVisualUpdate = () => { },
            RenderView = view
        };
        view.PrepareInitialFrame();

        var root = Attach(10, view, owner, null);
        pipeline.FlushLayout();
        var stack = GetStack(view);
        var child = Assert.Single(stack.Children);
        Assert.Equal(new Offset(10, 5), Assert.IsType<StackParentData>(child.ParentData).Offset);

        root = Attach(40, view, owner, root);
        owner.BuildScope();
        pipeline.FlushLayout();

        stack = GetStack(view);
        child = Assert.Single(stack.Children);
        Assert.Equal(new Offset(40, 5), Assert.IsType<StackParentData>(child.ParentData).Offset);
    }

    [Fact]
    public void UpdateRenderObject_AlignmentとFitを反映してLayoutDirtyにする()
    {
        var renderObject = new Stack
        {
            Alignment = Alignment.TopLeft,
            Fit = StackFit.Loose
        }.CreateRenderObject();
        renderObject.Layout(BoxConstraints.Tight(100, 80));
        Assert.False(renderObject.NeedsLayout);

        new Stack
        {
            Alignment = Alignment.BottomRight,
            Fit = StackFit.Expand
        }.UpdateRenderObject(renderObject);

        Assert.Equal(Alignment.BottomRight, renderObject.Alignment);
        Assert.Equal(StackFit.Expand, renderObject.Fit);
        Assert.True(renderObject.NeedsLayout);
    }

    private static RenderObjectToWidgetElement<RenderView> Attach(
        double left,
        RenderView view,
        BuildOwner owner,
        RenderObjectToWidgetElement<RenderView>? element)
    {
        return new RenderObjectToWidgetAdapter
        {
            Container = view,
            Child = new SizedBox
            {
                Width = 200,
                Height = 100,
                Child = new Stack
                {
                    Children =
                    [
                        new Positioned
                        {
                            Left = left,
                            Top = 5,
                            Width = 30,
                            Height = 20,
                            Child = new SizedBox()
                        }
                    ]
                }
            }
        }.AttachToRenderTree(owner, element);
    }

    private static RenderStack GetStack(RenderView view)
    {
        var constrainedBox = Assert.IsType<RenderConstrainedBox>(view.Child);
        return Assert.IsType<RenderStack>(constrainedBox.Child);
    }
}
