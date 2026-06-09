using FloatSoda.Elements;

namespace FloatSoda.Widgets;

public abstract record Widget
{
    public abstract Element CreateElement();
}

public abstract record SingleChildRenderObjectWidget : RenderObjectWidget
{
    public Widget? Child { get; init; }

    public override Element CreateElement()
    {
        throw new NotImplementedException();
    }
}