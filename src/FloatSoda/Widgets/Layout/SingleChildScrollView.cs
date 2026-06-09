using FloatSoda.Elements;

namespace FloatSoda.Widgets.Layout;

public record SingleChildScrollView : Widget
{
    public Widget? Child { get; init; } = null;
    
    public override Element CreateElement()
    {
        throw new NotImplementedException();
    }
}