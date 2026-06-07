using SkiaSharp;

namespace FloatSoda.Common.Layer;

public class TransformLayer : ContainerLayer
{
    public SKMatrix Transform { get; init; } = SKMatrix.Identity;

    public override void Layout(LayerContext context)
    {
        var childBounds = LayoutChildren(context);

        PaintBounds = Transform.MapRect(childBounds);
    }

    public override void Paint(LayerContext context)
    {
        context.Canvas.Save();
        context.Canvas.Concat(Transform);

        base.Paint(context);

        context.Canvas.Restore();
    }

    public override ILayer Clone()
    {
        var cloned = new TransformLayer()
        {
            Transform = Transform
        };

        foreach (var child in Children)
        {
            cloned.Children.Add(child.Clone());
        }

        return cloned;
    }
}