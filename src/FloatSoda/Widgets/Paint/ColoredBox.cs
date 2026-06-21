using FloatSoda.RenderObjects;
using FloatSoda.RenderObjects.Painting;
using SkiaSharp;

namespace FloatSoda.Widgets.Paint;

public record ColoredBox : SingleChildRenderObjectWidget
{
    public SKColor Color { get; init; }


    public override RenderObject CreateRenderObject()
    {
        return new RenderColoredBox
        {
            Color = Color
        };
    }
}