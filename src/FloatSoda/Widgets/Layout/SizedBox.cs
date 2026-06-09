using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.Render;
using FloatSoda.Render.Layout;

namespace FloatSoda.Widgets.Layout;

public record SizedBox : SingleChildRenderObjectWidget
{
    public double? Width { get; init; } = null;
    public double? Height { get; init; } = null;

    public override Element CreateElement()
    {
        throw new NotImplementedException();
    }

    public override RenderObject CreateRenderObject() => new RenderConstrainedBox
    {
        AdditionalConstraints = BoxConstraints.TightFor(Width, Height)
    };
}