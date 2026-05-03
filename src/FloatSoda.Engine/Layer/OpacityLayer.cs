using SkiaSharp;

namespace FloatSoda.Engine.Layer;

public class OpacityLayer : ContainerLayer
{
    public byte Alpha { get; set; } = 255;

    public override void Paint(LayerContext context)
    {
        var paint = new SKPaint()
        {
            Color = SKColors.Transparent.WithAlpha(Alpha),
            IsAntialias = true
        };

        context.Canvas.SaveLayer(PaintBounds, paint);

        base.Paint(context);

        context.Canvas.Restore();
    }
}