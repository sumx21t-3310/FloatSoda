using FloatSoda.Elements;

namespace FloatSoda.Widgets.Painting;

public record Opacity : Widget
{
    public Widget Child { get; init; }
    public override Element CreateElement()
    {
        throw new NotImplementedException();
    }
}