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

    /// <summary>
    /// 指定したWidgetツリーを単独のパイプラインでビルド・レイアウト・ペイントし、結果をBitmapへ描画します。
    /// </summary>
    /// <param name="widget">描画対象のルートとなるWidget。<see langword="null"/>は指定できません。</param>
    /// <param name="imageSize">出力するBitmapのピクセルサイズ。</param>
    /// <returns>描画結果を格納したBitmap。レイヤーが生成されなかった場合は空のBitmap。</returns>
    /// <remarks>
    /// ビルド・レイアウト・ペイントを1回だけ実行するため、非同期の完了を待つウィジェットは完了前の状態で描画されます。
    /// <c>Image</c>の画像まで描画するには、呼び出しの前に同じプロバイダーで
    /// <c>await provider.ResolveAsync()</c>を実行し、返されたハンドルを描画が終わるまで保持してください。
    /// 画像を借りているあいだは、<c>Image</c>の読み込みが同期的に完了します。
    /// </remarks>
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
        
        var root = new RenderObjectToWidgetAdapter
        {
            Container = renderView,
            Child = widget
        }.AttachToRenderTree(owner, null);

        try
        {
            pipeline.RenderView.PrepareInitialFrame();
            pipeline.FlushLayout();
            pipeline.FlushPaint();

            var layer = renderView.Layer?.Clone();
            return layer == null
                ? new SKBitmap(imageSize.Width, imageSize.Height)
                : _layerRenderer.Render(layer, imageSize);
        }
        finally
        {
            // Widgetツリーを外してStateを破棄する。外さないと、Imageが借りた画像のように
            // Stateが解放を担うリソースが、描画のたびに残り続ける。
            new RenderObjectToWidgetAdapter
            {
                Container = renderView,
                Child = null
            }.AttachToRenderTree(owner, root);
            owner.BuildScope();
        }
    }
}
