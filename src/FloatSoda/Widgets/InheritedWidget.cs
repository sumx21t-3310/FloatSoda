using FloatSoda.Elements;

namespace FloatSoda.Widgets;

public abstract record InheritedWidget : Widget
{
    public override Element CreateElement() => new InheritedElement();

    public abstract bool UpdateShouldNotify(InheritedWidget oldWidget);
}