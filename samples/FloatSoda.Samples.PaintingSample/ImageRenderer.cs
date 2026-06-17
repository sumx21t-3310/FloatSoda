using FloatSoda.Common.Layer;
using FloatSoda.Core;
using FloatSoda.Render;
using FloatSoda.Widgets;
using SkiaSharp;

namespace FloatSoda.Samples.PaintingSample;

public class ImageRenderer
{
    public void RenderWidgetTree(Widget widget, SKSizeI imageSize, string imagePath)
    {
        var renderView = new RenderView(imageSize.Width, imageSize.Height);
        var pipeline = new RenderPipeline
        {
            OnNeedVisualUpdate = () => { },
            RenderView = renderView
        };

        new RenderObjectToWidgetAdapter
        {
            Container = renderView,
            Child = widget
        }.AttachToRenderTree();
        
        pipeline.RenderView.PrepareInitialFrame();

        pipeline.FlushLayout();
        pipeline.FlushPaint();

        var layer = renderView.Layer?.Clone();
        if (layer == null) return;
        RenderLayerTree(layer, imageSize, imagePath);
    }

    public void RenderObjectTree(RenderBox renderObject, SKSizeI imageSize, string imagePath)
    {
        var pipeline = new RenderPipeline
        {
            OnNeedVisualUpdate = () => { },
            RenderView = new RenderView(imageSize.Width, imageSize.Height)
            {
                Child = renderObject
            }
        };
        
        pipeline.RenderView.PrepareInitialFrame();
        
        pipeline.FlushLayout();
        pipeline.FlushPaint();

        var layer = pipeline.RenderView.Layer?.Clone();

        if (layer == null) return;
        RenderLayerTree(layer, imageSize, imagePath);
    }

    public void RenderLayerTree(ILayer root, SKSizeI imageSize, string savePath)
    {
        SKImageInfo info = new(imageSize.Width, imageSize.Height);
        using var surface = SKSurface.Create(info);

        surface.Canvas.Clear(SKColors.Transparent);
        var renderContext = LayerContext.Create(surface);

        root.Paint(renderContext);

        var image = surface.Snapshot();

        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.Create(savePath);
        data.SaveTo(stream);
    }
}