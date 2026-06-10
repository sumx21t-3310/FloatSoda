using FloatSoda.Elements;

namespace FloatSoda.Widgets.Layout;

public record Padding : Widget
{
    public Widget Child { get; init; }

    public override Element CreateElement()
    {
        throw new NotImplementedException();
    }
}