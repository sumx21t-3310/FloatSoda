using FloatSoda.Rendering.Layers;
using SkiaSharp;

namespace FloatSoda.Rendering;

/// <summary>
/// LayerツリーをCPU上のBitmapへ描画します。
/// </summary>
public sealed class LayerBitmapRenderer
{
    public SKBitmap Render(ILayer root, SKSizeI imageSize)
    {
        ArgumentNullException.ThrowIfNull(root);

        if (imageSize.Width <= 0 || imageSize.Height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(imageSize), "幅と高さは正の値である必要があります。");
        }

        var info = new SKImageInfo(imageSize.Width, imageSize.Height);
        using var surface = SKSurface.Create(info);
        surface.Canvas.Clear(SKColors.Transparent);

        LayerRenderer.Render(root, LayerContext.Create(surface));

        var bitmap = new SKBitmap(info);
        using var image = surface.Snapshot();
        image.ReadPixels(bitmap.Info, bitmap.GetPixels(), bitmap.RowBytes, 0, 0);
        return bitmap;
    }
}
