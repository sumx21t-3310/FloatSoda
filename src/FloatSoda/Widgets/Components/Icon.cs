using FloatSoda.Elements;
using SkiaSharp;

namespace FloatSoda.Widgets.Components;

public record Icon : Widget
{
    public SKColor Color { get; init; }
    public double Size { get; init; }
    
    
    public override Element CreateElement()
    {
        throw new NotImplementedException();
    }
}