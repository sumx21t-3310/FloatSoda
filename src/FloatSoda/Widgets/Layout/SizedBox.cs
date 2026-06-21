using FloatSoda.Geometrics;
using FloatSoda.RenderObjects;
using FloatSoda.RenderObjects.Layout;

namespace FloatSoda.Widgets.Layout;

public record SizedBox : SingleChildRenderObjectWidget<RenderConstrainedBox>
{
    public double? Width { get; init; } = null;
    public double? Height { get; init; } = null;


    public override RenderConstrainedBox CreateRenderObject()
    {
        return new RenderConstrainedBox
        {
            AdditionalConstraints = BoxConstraints.TightFor(Width, Height)
        };
    }
}