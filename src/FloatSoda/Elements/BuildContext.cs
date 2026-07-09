using FloatSoda.Widgets;

namespace FloatSoda.Elements;

public interface IBuildContext
{
    Widget Widget { get; }
    BuildOwner? Owner { get; }
    T? DependOnInheritedWidgetOfExactType<T>() where T : InheritedWidget;

    InheritedElement? GetElementForInheritedWidgetOfExactType<T>() where T : InheritedWidget;
}