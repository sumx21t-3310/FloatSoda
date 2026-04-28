using SkiaSharp;

namespace FloatSoda.Render;

public class BoxElement(Element? child = null) : Element(child)
{
    public Size Size { get; init; }
    public Vector2 Position { get; init; }
    public SKColor Color { get; init; } = SKColors.Transparent;
    public Bound Bound { get; }

    protected override void OnDraw(RenderContext context)
    {
        context.Paint.Color = Color;
        context.Paint.Style = SKPaintStyle.Fill;

        var offset = Bound.Pivot + Position;

        context.Canvas.DrawRect(offset.X, offset.Y, Size.Width, Size.Height, context.Paint);
    }
}