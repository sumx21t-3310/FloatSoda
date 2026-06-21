using FloatSoda.Elements;
using FloatSoda.RenderObjects;
using FloatSoda.RenderObjects.Painting;

namespace FloatSoda.Widgets;

public abstract record SingleChildRenderObjectWidget<T> : RenderObjectWidget<T> where T : RenderObject
{
    public Widget? Child { get; init; }

    public override Element CreateElement() => new SingleChildRenderObjectElement<T>
    {
        Widget = this
    };
}