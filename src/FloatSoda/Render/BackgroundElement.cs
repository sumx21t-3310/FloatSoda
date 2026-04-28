using SkiaSharp;

namespace FloatSoda.Render;

public class BackgroundElement(SKColor color, Element? child = null) : Element(child)
{
    protected override void OnDraw(RenderContext context) => context.Canvas.Clear(color);
}