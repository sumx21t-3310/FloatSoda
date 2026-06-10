using FloatSoda.Elements;
using FloatSoda.Geometrics;

namespace FloatSoda.Widgets.Layout;

public record Row : Widget
{
    public MainAxisAlignment MainAxisAlignment { get; init; } = MainAxisAlignment.Start;
    public CrossAxisAlignment CrossAxisAlignment { get; init; } = CrossAxisAlignment.Start;
    public List<Widget> Children { get; init; } = [];
    
    public override Element CreateElement()
    {
        throw new NotImplementedException();
    }
}