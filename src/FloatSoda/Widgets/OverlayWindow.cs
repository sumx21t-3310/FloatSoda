using FloatSoda.Elements;
using FloatSoda.OVR.Overlay;

namespace FloatSoda.Widgets;

public record OverlayWindow : Widget
{
    public Widget? Child { get; init; }
    public IOverlay Overlay { get; init; }

    public override Element CreateElement()
    {
        throw new NotImplementedException();
    }
}