using FloatSoda.Elements;

namespace FloatSoda.Widgets;

public abstract record Widget
{
    public abstract Element CreateElement();
}