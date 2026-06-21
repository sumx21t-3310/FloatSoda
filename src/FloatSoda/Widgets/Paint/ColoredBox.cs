using FloatSoda.RenderObjects.Painting;
using SkiaSharp;

namespace FloatSoda.Widgets.Paint;

public record ColoredBox : SingleChildRenderObjectWidget<RenderColoredBox>
{
    public SKColor Color { get; init; }


    public override RenderColoredBox CreateRenderObject()
    {
        return new RenderColoredBox
        {
            Color = Color
        };
    }
}