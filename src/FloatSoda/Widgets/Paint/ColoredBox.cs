using FloatSoda.Geometrics;
using FloatSoda.RenderObjects.Painting;

namespace FloatSoda.Widgets.Paint;

public record ColoredBox : SingleChildRenderObjectWidget<RenderColoredBox>
{
    public Color Color { get; init; }
    
    public override RenderColoredBox CreateRenderObject()
    {
        return new RenderColoredBox
        {
            Color = Color
        };
    }

    public override void UpdateRenderObject(RenderColoredBox renderObject) => renderObject.Color = Color;
}