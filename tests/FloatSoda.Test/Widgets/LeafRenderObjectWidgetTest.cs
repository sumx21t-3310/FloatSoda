using FloatSoda.Elements;
using FloatSoda.RenderObjects.Painting;
using FloatSoda.Widgets;
using SkiaSharp;

namespace FloatSoda.Test.Widgets;

public class LeafRenderObjectWidgetTest
{
    [Fact]
    public void CreateElement_AssignsWidgetToLeafElement()
    {
        var widget = new TestLeafWidget { Color = SKColors.Red };

        var element = Assert.IsType<LeafRenderObjectElement<RenderColoredBox>>(widget.CreateElement());

        Assert.Same(widget, element.Widget);
    }

    [Fact]
    public void Mount_CreatesConfiguredRenderObject()
    {
        var widget = new TestLeafWidget { Color = SKColors.Red };
        var element = widget.CreateElement();

        element.Mount(parent: null);

        var renderObject = Assert.IsType<RenderColoredBox>(element.RenderObject);
        Assert.Equal(SKColors.Red, renderObject.Color);
        Assert.False(element.Dirty);
    }

    [Fact]
    public void Update_ReusesAndUpdatesRenderObject()
    {
        var element = new TestLeafWidget { Color = SKColors.Red }.CreateElement();
        element.Mount(parent: null);
        var originalRenderObject = Assert.IsType<RenderColoredBox>(element.RenderObject);

        element.Update(new TestLeafWidget { Color = SKColors.Blue });

        Assert.Same(originalRenderObject, element.RenderObject);
        Assert.Equal(SKColors.Blue, originalRenderObject.Color);
        Assert.False(element.Dirty);
    }

    [Fact]
    public void VisitChildren_DoesNotVisitAnyElement()
    {
        var element = new TestLeafWidget().CreateElement();
        element.Mount(parent: null);
        var visits = 0;

        element.VisitChildren(_ => visits++);

        Assert.Equal(0, visits);
    }

    private sealed record TestLeafWidget : LeafRenderObjectWidget<RenderColoredBox>
    {
        public SKColor Color { get; init; } = SKColors.Black;

        public override RenderColoredBox CreateRenderObject() => new() { Color = Color };

        public override void UpdateRenderObject(RenderColoredBox renderObject) => renderObject.Color = Color;
    }
}
