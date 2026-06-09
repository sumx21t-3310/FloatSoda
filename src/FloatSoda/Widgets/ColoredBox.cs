using FloatSoda.Elements;
using FloatSoda.Render;
using FloatSoda.Render.Painting;
using SkiaSharp;

namespace FloatSoda.Widgets;

public record ColoredBox : SingleChildRenderObjectWidget
{
    public SKColor Color { get; init; }

    public override Element CreateElement()
    {
        throw new NotImplementedException();
    }

    public override RenderObject CreateRenderObject() => new RenderColoredBox
    {
        Color = Color
    };
}