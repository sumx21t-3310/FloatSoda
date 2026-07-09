using FloatSoda.Elements;

namespace FloatSoda.Widgets;

public abstract record InheritedWidget : Widget
{
    public required Widget Child { get; init; }

    public override Element CreateElement() => new InheritedElement() { Widget = this };

    public abstract bool UpdateShouldNotify(InheritedWidget oldWidget);
}