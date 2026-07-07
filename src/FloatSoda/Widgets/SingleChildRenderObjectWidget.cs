using FloatSoda.Elements;
using FloatSoda.RenderObjects;

namespace FloatSoda.Widgets;

public abstract record SingleChildRenderObjectWidget<T> : RenderObjectWidget<T> where T : RenderObject
{
    public Widget? Child { get; init; }

    public override Element CreateElement() => new SingleChildRenderObjectElement<T>
    {
        Widget = this
    };
}