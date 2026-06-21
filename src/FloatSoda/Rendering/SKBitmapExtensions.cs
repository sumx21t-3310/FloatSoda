using SkiaSharp;

namespace FloatSoda.Rendering;

public static class SKBitmapExtensions
{
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