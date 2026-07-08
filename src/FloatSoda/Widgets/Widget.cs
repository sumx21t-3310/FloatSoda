using FloatSoda.Elements;

namespace FloatSoda.Widgets;

public abstract record Widget
{
    public IKey? Key { get; init; }
    
    public static bool CanUpdate(Widget oldWidget, Widget newWidget)
    {
        return oldWidget.GetType() == newWidget.GetType() && Equals(oldWidget.Key, newWidget.Key);
    }

    public abstract Element CreateElement();

    public override int GetHashCode() => (Key != null ? Key.GetHashCode() : 0);
}