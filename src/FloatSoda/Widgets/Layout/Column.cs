using FloatSoda.Elements;

namespace FloatSoda.Widgets.Layout;

public record Column : Widget
{
    public List<Widget> Children { get; init; } = [];
    public override Element CreateElement()
    {
        throw new NotImplementedException();
    }
}