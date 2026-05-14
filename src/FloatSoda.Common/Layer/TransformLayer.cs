using SkiaSharp;

namespace FloatSoda.Common.Layer;

public class TransformLayer : ContainerLayer
{
    public SKMatrix Transform { get; set; } = SKMatrix.Identity;
    
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
}