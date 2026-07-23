using SkiaSharp;

namespace FloatSoda.Rendering;

/// <summary>
/// ビットマップの保存操作を提供します。
/// </summary>
public static class SKBitmapExtensions
{
    /// <summary>
    /// ビットマップを指定された形式で符号化し、ファイルへ保存します。
    /// </summary>
    /// <param name="bitmap">
    /// 保存するビットマップ。所有権は呼び出し元に残ります。
    /// </param>
    /// <param name="path">
    /// 保存先のファイルパス。既存のファイルは上書きされます。
    /// </param>
    /// <param name="format">
    /// 出力する画像形式。既定値は<see cref="SKEncodedImageFormat.Png"/>です。
    /// </param>
    /// <param name="quality">
    /// 符号化品質を表す0から100までの値。既定値は100で、解釈は画像形式に依存します。
    /// </param>
    /// <remarks>
    /// 相対パスは現在の作業ディレクトリを基準に解決されます。
    /// 保存先の親ディレクトリは作成しません。
    /// </remarks>
    public static void Save(
        this SKBitmap bitmap,
        string path,
        SKEncodedImageFormat format = SKEncodedImageFormat.Png,
        int quality = 100)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(format, quality);
        using var stream = File.Create(path);
        data.SaveTo(stream);
    }
}