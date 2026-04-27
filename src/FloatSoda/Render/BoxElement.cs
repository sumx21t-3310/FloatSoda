using SkiaSharp;

namespace FloatSoda.Render;

public class BoxElement(Element? child = null) : Element(child)
{
    public Size Size { get; set; }
    public Vector2 Position { get; set; }
    public SKColor Color { get; set; } = SKColors.Transparent;
    public Bound Bound { get; set; }

    protected override void OnDraw(RenderContext context)
    {
        context.Paint.Color = Color;
        context.Paint.Style = SKPaintStyle.Fill;

        var offset = Bound.Pivot + Position;
        
        context.Canvas.DrawRect(offset.X, offset.Y, Size.Width, Size.Height, context.Paint);
    }
}