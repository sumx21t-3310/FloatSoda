using FloatSoda.Elements;

namespace FloatSoda.Widgets;

public abstract record Widget
{
    public static bool CanUpdate(Widget oldWidget, Widget newWidget) => oldWidget == newWidget;
    public abstract Element CreateElement();
}