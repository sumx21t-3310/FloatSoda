using FloatSoda.Geometrics;
using FloatSoda.RenderObjects.Layout;

namespace FloatSoda.Widgets.Layout;

public record ConstrainedBox : SingleChildRenderObjectWidget<RenderConstrainedBox>
{
    public BoxConstraints  Constraints { get; init; }
    public override RenderConstrainedBox CreateRenderObject() => new RenderConstrainedBox
    {
        AdditionalConstraints = Constraints
    };
}