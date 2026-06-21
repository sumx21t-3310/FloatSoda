using FloatSoda.Elements;

namespace FloatSoda.Widgets;

public abstract record MultiChildRenderObjectWidget : RenderObjectWidget
{
    public List<Widget> Children { get; init; } = [];

    public override Element CreateElement()
    {
        return new MultiChildRenderObjectElement
        {
            Widget = this
        };
    }
}