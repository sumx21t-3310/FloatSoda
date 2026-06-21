using FloatSoda.Elements;
using FloatSoda.RenderObjects;
using FloatSoda.RenderObjects.Painting;

namespace FloatSoda.Widgets;

public abstract record SingleChildRenderObjectWidget : RenderObjectWidget
{
    public Widget? Child { get; init; }

    public override Element CreateElement() => new SingleChildRenderObjectElement
    {
        Widget = this
    };
}