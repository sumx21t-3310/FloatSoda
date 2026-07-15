using FloatSoda.Core;
using FloatSoda.Elements;
using FloatSoda.Rendering;
using FloatSoda.RenderObjects;
using FloatSoda.Widgets;
using SkiaSharp;

namespace FloatSoda.Testing;

/// <summary>
/// Widgetツリーをビルド・レイアウト・ペイントし、Bitmapへ描画します。
/// </summary>
public sealed class WidgetBitmapRenderer
{
    private readonly LayerBitmapRenderer _layerRenderer = new();

    public SKBitmap Render(Widget widget, SKSizeI imageSize)
    {
        ArgumentNullException.ThrowIfNull(widget);

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
        return layer == null
            ? new SKBitmap(imageSize.Width, imageSize.Height)
            : _layerRenderer.Render(layer, imageSize);
    }
}
