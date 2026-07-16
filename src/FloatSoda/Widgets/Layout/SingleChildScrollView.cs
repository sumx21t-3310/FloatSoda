using FloatSoda.Elements;

namespace FloatSoda.Widgets.Layout;

internal record SingleChildScrollView : Widget
{
    public Widget? Child { get; init; } = null;
    
    public override Element CreateElement()
    {
        throw new NotImplementedException();
    }
}
