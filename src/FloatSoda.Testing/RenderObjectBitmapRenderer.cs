using FloatSoda.Core;
using FloatSoda.Rendering;
using FloatSoda.RenderObjects;
using SkiaSharp;

namespace FloatSoda.Testing;

/// <summary>
/// RenderObjectツリーをレイアウト・ペイントし、Bitmapへ描画します。
/// </summary>
public sealed class RenderObjectBitmapRenderer
{
    private readonly LayerBitmapRenderer _layerRenderer = new();

    public SKBitmap Render(RenderBox renderObject, SKSizeI imageSize)
    {
        ArgumentNullException.ThrowIfNull(renderObject);

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
        return layer == null
            ? new SKBitmap(imageSize.Width, imageSize.Height)
            : _layerRenderer.Render(layer, imageSize);
    }
}
