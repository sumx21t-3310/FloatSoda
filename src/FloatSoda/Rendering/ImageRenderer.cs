using FloatSoda.Common.Layer;
using FloatSoda.Core;
using FloatSoda.Elements;
using FloatSoda.RenderObjects;
using FloatSoda.Widgets;
using SkiaSharp;

namespace FloatSoda.Rendering;

public class ImageRenderer
{
    public SKBitmap RenderWidgetTree(Widget widget, SKSizeI imageSize)
    {
        var renderView = new RenderView(imageSize.Width, imageSize.Height);
        var pipeline = new RenderPipeline
        {
            OnNeedVisualUpdate = () => { },
            RenderView = renderView
        };

        var owner = new BuildOwner(() => { });
        
        new RenderObjectToWidgetAdapter
        {
            Container = renderView,
            Child = widget
        }.AttachToRenderTree(owner, null);

        pipeline.RenderView.PrepareInitialFrame();
        pipeline.FlushLayout();
        pipeline.FlushPaint();

        var layer = renderView.Layer?.Clone();
        if (layer == null) return new SKBitmap(imageSize.Width, imageSize.Height);
        return RenderLayerTree(layer, imageSize);
    }

    public SKBitmap RenderObjectTree(RenderBox renderObject, SKSizeI imageSize)
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
        if (layer == null) return new SKBitmap(imageSize.Width, imageSize.Height);
        return RenderLayerTree(layer, imageSize);
    }

    public SKBitmap RenderLayerTree(ILayer root, SKSizeI imageSize)
    {
        var info = new SKImageInfo(imageSize.Width, imageSize.Height);
        using var surface = SKSurface.Create(info);
        surface.Canvas.Clear(SKColors.Transparent);

        var renderContext = LayerContext.Create(surface);
        root.Paint(renderContext);

        var bitmap = new SKBitmap(info);
        using var image = surface.Snapshot();
        image.ReadPixels(bitmap.Info, bitmap.GetPixels(), bitmap.RowBytes, 0, 0);
        return bitmap;
    }
}