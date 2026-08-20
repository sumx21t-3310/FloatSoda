using FloatSoda.Core.Providers;
using FloatSoda.Geometrics;
using FloatSoda.RenderObjects.Painting;
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
    private static string CreateTempPng(SKColor color, int width = 8, int height = 8)
    {
        var path = Path.Combine(Path.GetTempPath(), $"floatsoda-image-{Guid.NewGuid():N}.png");
        using var bitmap = new SKBitmap(width, height);
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

    [Fact]
    public void Fit_既定は横長画像の縦横比を維持し上下に余白ができる()
    {
        var path = CreateTempPng(SKColors.Blue, 8, 4);

        try
        {
            using var bitmap = Renderer.Render(
                new SizedBox
                {
                    Width = Size.Width,
                    Height = Size.Height,
                    Child = new ImageWidget { Provider = new FileImageProvider(path) }
                },
                Size);

            // 8x4の画像を40x40へContainで収めると40x20になり、上下へ10pxずつ余白ができる。
            Assert.Equal(SKColors.Blue, bitmap.GetPixel(20, 20));
            Assert.Equal(default, bitmap.GetPixel(20, 2));
            Assert.Equal(default, bitmap.GetPixel(20, 37));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Fit_Fill_領域全体を埋める()
    {
        var path = CreateTempPng(SKColors.Blue, 8, 4);

        try
        {
            using var bitmap = Renderer.Render(
                new SizedBox
                {
                    Width = Size.Width,
                    Height = Size.Height,
                    Child = new ImageWidget { Provider = new FileImageProvider(path), Fit = BoxFit.Fill }
                },
                Size);

            Assert.Equal(SKColors.Blue, bitmap.GetPixel(20, 2));
            Assert.Equal(SKColors.Blue, bitmap.GetPixel(20, 37));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Fit_None_画像を拡大せず原寸で中央へ配置する()
    {
        var path = CreateTempPng(SKColors.Blue, 8, 8);

        try
        {
            using var bitmap = Renderer.Render(
                new SizedBox
                {
                    Width = Size.Width,
                    Height = Size.Height,
                    Child = new ImageWidget { Provider = new FileImageProvider(path), Fit = BoxFit.None }
                },
                Size);

            // 8x8が中央(16,16)-(24,24)へ置かれ、その外側は描画されない。
            Assert.Equal(SKColors.Blue, bitmap.GetPixel(20, 20));
            Assert.Equal(default, bitmap.GetPixel(2, 2));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Alignment_TopLeft_収めた画像を上端へ寄せる()
    {
        var path = CreateTempPng(SKColors.Blue, 8, 4);

        try
        {
            using var bitmap = Renderer.Render(
                new SizedBox
                {
                    Width = Size.Width,
                    Height = Size.Height,
                    Child = new ImageWidget
                    {
                        Provider = new FileImageProvider(path),
                        Alignment = Alignment.TopLeft
                    }
                },
                Size);

            // Containで40x20になった画像が上端へ寄るため、上が塗られ下が余白になる。
            Assert.Equal(SKColors.Blue, bitmap.GetPixel(20, 2));
            Assert.Equal(default, bitmap.GetPixel(20, 37));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Fit_Cover_描画元を切り取って領域全体を埋める()
    {
        // 左半分が赤、右半分が青の16x4を40x40へCoverで収める。
        // Sourceは中央の4x4へ切り取られるので、赤と青の境目が領域の中央に来る。
        // 切り取りではなく引き伸ばしなら、境目は中央に来ない。
        var path = Path.Combine(Path.GetTempPath(), $"floatsoda-image-{Guid.NewGuid():N}.png");
        using (var bitmap = new SKBitmap(16, 4))
        {
            for (var x = 0; x < 16; x++)
            {
                for (var y = 0; y < 4; y++)
                {
                    bitmap.SetPixel(x, y, x < 8 ? SKColors.Red : SKColors.Blue);
                }
            }

            using var encoded = SKImage.FromBitmap(bitmap);
            using var data = encoded.Encode(SKEncodedImageFormat.Png, 100);
            using var stream = File.Create(path);
            data.SaveTo(stream);
        }

        try
        {
            using var rendered = Renderer.Render(
                new SizedBox
                {
                    Width = Size.Width,
                    Height = Size.Height,
                    Child = new ImageWidget { Provider = new FileImageProvider(path), Fit = BoxFit.Cover }
                },
                Size);

            // 領域全体が埋まる(余白が残らない)。
            Assert.Equal(SKColors.Red, rendered.GetPixel(2, 2));
            Assert.Equal(SKColors.Blue, rendered.GetPixel(37, 37));
            // 切り取り後の境目が中央に来る。
            Assert.Equal(SKColors.Red, rendered.GetPixel(18, 20));
            Assert.Equal(SKColors.Blue, rendered.GetPixel(22, 20));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Fit_定義されていない値_ArgumentOutOfRangeExceptionを投げる()
    {
        using var image = CreateSolidImage();

        Assert.Throws<ArgumentOutOfRangeException>(() => new RenderImage { Image = image, Fit = (BoxFit)99 });
    }

    [Fact]
    public void Alignment_成分が有限値でない_ArgumentOutOfRangeExceptionを投げる()
    {
        using var image = CreateSolidImage();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new RenderImage { Image = image, Alignment = new Alignment(float.NaN, 0) });
    }

    private static SKImage CreateSolidImage()
    {
        using var bitmap = new SKBitmap(8, 8);
        bitmap.Erase(SKColors.Blue);
        return SKImage.FromBitmap(bitmap);
    }
}
