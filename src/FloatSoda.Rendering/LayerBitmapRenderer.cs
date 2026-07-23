using FloatSoda.Rendering.Layers;
using SkiaSharp;

namespace FloatSoda.Rendering;

/// <summary>
/// LayerツリーをCPU上のBitmapへ描画します。
/// </summary>
public sealed class LayerBitmapRenderer
{
    /// <summary>
    /// レイヤーツリーをレイアウトし、透明で初期化した新しいビットマップへ描画します。
    /// </summary>
    /// <param name="root">
    /// 描画するレイヤーツリーのルート。<see langword="null"/>は指定できません。
    /// 所有権は呼び出し元に残ります。
    /// </param>
    /// <param name="imageSize">
    /// 出力するビットマップのピクセル単位の大きさ。幅と高さには正の値を指定します。
    /// </param>
    /// <returns>
    /// 描画結果を保持する新しいビットマップ。呼び出し元が破棄する必要があります。
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="root"/>が<see langword="null"/>の場合にスローされます。
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="imageSize"/>の幅または高さが0以下の場合にスローされます。
    /// </exception>
    /// <remarks>
    /// レイアウトにより、ツリー内の各レイヤーの描画境界が更新されます。
    /// </remarks>
    /// <seealso cref="LayerRenderer"/>
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
