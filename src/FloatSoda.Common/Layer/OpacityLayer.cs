using SkiaSharp;

namespace FloatSoda.Common.Layer;

public class OpacityLayer : ContainerLayer
{
    public byte Alpha { get; set; } = 255;

    public override void Paint(LayerContext context)
    {
        var paint = new SKPaint()
        {
            Color = SKColors.White.WithAlpha(Alpha),
            IsAntialias = true
        };

        context.Canvas.SaveLayer(paint);

        base.Paint(context);

        context.Canvas.Restore();
    }

    public override ILayer Clone()
    {
        var cloned = new OpacityLayer();

        foreach (var child in Children)
        {
            cloned.Children.Add(child.Clone());
        }

        return cloned;
    }
}