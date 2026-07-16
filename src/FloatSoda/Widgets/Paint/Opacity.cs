using FloatSoda.Elements;

namespace FloatSoda.Widgets.Paint;

internal record Opacity : Widget
{
    public Widget Child { get; init; }
    public override Element CreateElement()
    {
        throw new NotImplementedException();
    }
}
