using FloatSoda.Core.Providers;
using FloatSoda.Testing;
using FloatSoda.Widgets.Layout;
using SkiaSharp;
using ImageWidget = FloatSoda.Widgets.Paint.Image;

namespace FloatSoda.Test.Widgets;

public class ImageTest
{
    private static readonly WidgetBitmapRenderer Renderer = new();
    private static readonly SKSizeI Size = new(40, 40);

    /// <summary>指定した単色で塗りつぶしたPNGを一時ファイルへ書き出す。</summary>
    private static string CreateTempPng(SKColor color)
    {
        var path = Path.Combine(Path.GetTempPath(), $"floatsoda-image-{Guid.NewGuid():N}.png");
        using var bitmap = new SKBitmap(8, 8);
        bitmap.Erase(color);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.Create(path);
        data.SaveTo(stream);
        return path;
    }

    [Fact]
    public void Render_ヘッドレスレンダラーで描画_1パスで画像が反映される()
    {
        var path = CreateTempPng(SKColors.Blue);

        try
        {
            var widget = new SizedBox
            {
                Width = Size.Width,
                Height = Size.Height,
                Child = new ImageWidget { Provider = new FileImageProvider(path) }
            };

            using var bitmap = Renderer.Render(widget, Size);

            Assert.Equal(SKColors.Blue, bitmap.GetPixel(20, 20));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Render_ファイルが存在しない_例外を投げずOnErrorへ通知する()
    {
        var path = Path.Combine(Path.GetTempPath(), $"floatsoda-missing-{Guid.NewGuid():N}.png");
        Exception? reported = null;

        var widget = new SizedBox
        {
            Width = Size.Width,
            Height = Size.Height,
            Child = new ImageWidget
            {
                Provider = new FileImageProvider(path),
                OnError = exception => reported = exception
            }
        };

        using var bitmap = Renderer.Render(widget, Size);

        Assert.IsType<FileNotFoundException>(reported);
        Assert.Equal(default, bitmap.GetPixel(20, 20));
    }

    [Fact]
    public void Load_画像として解釈できないファイル_InvalidOperationExceptionを投げる()
    {
        var path = Path.Combine(Path.GetTempPath(), $"floatsoda-broken-{Guid.NewGuid():N}.png");
        File.WriteAllText(path, "これはPNGではありません");

        try
        {
            Exception? reported = null;

            var widget = new ImageWidget
            {
                Provider = new FileImageProvider(path),
                OnError = exception => reported = exception
            };

            using var bitmap = Renderer.Render(widget, Size);

            Assert.IsType<InvalidOperationException>(reported);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
