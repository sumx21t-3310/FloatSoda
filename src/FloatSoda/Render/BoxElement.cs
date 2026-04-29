using System.Numerics;
using SkiaSharp;

namespace FloatSoda.Render;

public class BoxElement(Element? child = null) : Element(child)
{
    public Size Size { get; init; }
    public Vector2 Position { get; init; }
    public SKColor Color { get; init; } = SKColors.Transparent;


    protected override void OnDraw(RenderContext context)
    {
        context.Paint.Color = Color;
        context.Paint.Style = SKPaintStyle.Fill;

        context.Canvas.DrawRect(0, 0, Size.Width, Size.Height, context.Paint);
    }
}