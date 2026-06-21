using FloatSoda.Elements;
using FloatSoda.RenderObjects;

namespace FloatSoda.Widgets;

public abstract record MultiChildRenderObjectWidget<T> : RenderObjectWidget<T> where T : RenderObject
{
    public List<Widget> Children { get; init; } = [];

    public override Element CreateElement()
    {
        return new MultiChildRenderObjectElement<T>
        {
            Widget = this
        };
    }
}