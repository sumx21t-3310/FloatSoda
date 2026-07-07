using FloatSoda.Widgets;

namespace FloatSoda.Elements;

public interface IBuildContext
{
    Widget Widget { get; }
    BuildOwner? Owner { get; }
}