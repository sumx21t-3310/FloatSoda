using FloatSoda.Rendering.Layers;
using SkiaSharp;

namespace FloatSoda.Rendering.Test.Layers;

public class LayerTest
{
    private static readonly LayerBitmapRenderer Renderer = new();
    private static readonly SKSizeI Size = new(100, 100);

    private static PictureLayer MakePicture(SKColor color, SKRect rect)
    {
        using var recorder = new SKPictureRecorder();
        var canvas = recorder.BeginRecording(rect);
        using var paint = new SKPaint { Color = color };
        canvas.DrawRect(rect, paint);
        return new PictureLayer { Picture = recorder.EndRecording() };
    }

    [Fact]
    public void PictureLayer_PaintsColor()
    {
        var layer = MakePicture(SKColors.Red, SKRect.Create(0, 0, 100, 100));

        using var bitmap = Renderer.Render(layer, Size);

        Assert.Equal(SKColors.Red, bitmap.GetPixel(50, 50));
    }

    [Fact]
    public void ContainerLayer_PaintsAllChildren()
    {
        var container = new ContainerLayer();
        container.Children.Add(MakePicture(SKColors.Red, SKRect.Create(0, 0, 50, 100)));
        container.Children.Add(MakePicture(SKColors.Blue, SKRect.Create(50, 0, 50, 100)));

        using var bitmap = Renderer.Render(container, Size);

        Assert.Equal(SKColors.Red, bitmap.GetPixel(25, 50));
        Assert.Equal(SKColors.Blue, bitmap.GetPixel(75, 50));
    }

    [Fact]
    public void ClipRectLayer_ClipsToRect()
    {
        var clip = new ClipRectLayer(SKRect.Create(0, 0, 50, 100)) { ClipBehavior = Clip.HardEdge };
        clip.Children.Add(MakePicture(SKColors.Red, SKRect.Create(0, 0, 100, 100)));

        using var bitmap = Renderer.Render(clip, Size);

        Assert.Equal(SKColors.Red, bitmap.GetPixel(25, 50));
        Assert.Equal(SKColors.Empty, bitmap.GetPixel(75, 50));
    }

    [Fact]
    public void ClipRoundRectLayer_ClipsCorners()
    {
        var roundRect = new SKRoundRect(SKRect.Create(0, 0, 100, 100), 40, 40);
        var clip = new ClipRoundRectLayer(roundRect) { ClipBehavior = Clip.HardEdge };
        clip.Children.Add(MakePicture(SKColors.Red, SKRect.Create(0, 0, 100, 100)));

        using var bitmap = Renderer.Render(clip, Size);

        Assert.Equal(SKColors.Red, bitmap.GetPixel(50, 50));
        Assert.Equal(SKColors.Empty, bitmap.GetPixel(2, 2));
    }

    [Fact]
    public void ClipPathLayer_ClipsToPath()
    {
        using var path = new SKPath();
        path.AddRect(SKRect.Create(0, 0, 50, 100));
        var clip = new ClipPathLayer(path) { ClipBehavior = Clip.HardEdge };
        clip.Children.Add(MakePicture(SKColors.Red, SKRect.Create(0, 0, 100, 100)));

        using var bitmap = Renderer.Render(clip, Size);

        Assert.Equal(SKColors.Red, bitmap.GetPixel(25, 50));
        Assert.Equal(SKColors.Empty, bitmap.GetPixel(75, 50));
    }

    [Fact]
    public void OpacityLayer_AppliesAlpha()
    {
        var opacity = new OpacityLayer { Alpha = 128 };
        opacity.Children.Add(MakePicture(SKColors.Red, SKRect.Create(0, 0, 100, 100)));

        using var bitmap = Renderer.Render(opacity, Size);
        var pixel = bitmap.GetPixel(50, 50);

        Assert.InRange(pixel.Alpha, 120, 136);
        Assert.True(pixel.Red > 0);
    }

    [Fact]
    public void OpacityLayer_Clone_PreservesAlpha()
    {
        // レンダースレッドへはClone済みのLayerツリーが渡るため、Alphaの複製漏れは実描画に直結する(回帰テスト)
        var opacity = new OpacityLayer { Alpha = 128 };
        opacity.Children.Add(MakePicture(SKColors.Red, SKRect.Create(0, 0, 100, 100)));

        var cloned = (OpacityLayer)opacity.Clone();

        Assert.Equal(128, cloned.Alpha);
        Assert.Single(cloned.Children);
    }

    [Fact]
    public void TransformLayer_TranslatesChildren()
    {
        var transform = new TransformLayer { Transform = SKMatrix.CreateTranslation(50, 50) };
        transform.Children.Add(MakePicture(SKColors.Blue, SKRect.Create(0, 0, 10, 10)));

        using var bitmap = Renderer.Render(transform, Size);

        Assert.Equal(SKColors.Blue, bitmap.GetPixel(55, 55));
        Assert.Equal(SKColors.Empty, bitmap.GetPixel(5, 5));
    }

    private static ContainerLayer MakeClipLayer(string kind, SKRect clipRect) =>
        kind switch
        {
            "rect" => new ClipRectLayer(clipRect) { ClipBehavior = Clip.HardEdge },
            "rrect" => new ClipRoundRectLayer(new SKRoundRect(clipRect, 8))
            {
                ClipBehavior = Clip.HardEdge,
            },
            _ => new ClipPathLayer(MakePath(clipRect)) { ClipBehavior = Clip.HardEdge },
        };

    private static SKPath MakePath(SKRect rect)
    {
        var path = new SKPath();
        path.AddRect(rect);
        return path;
    }

    [Theory]
    [InlineData("rect")]
    [InlineData("rrect")]
    [InlineData("path")]
    public void Layout_呼び出し元がSave済み_SaveCountを変えない(string kind)
    {
        // #296: Layout が対になる Save なしに Restore し、呼び出し元の状態を1段外していた(回帰テスト)
        var clip = MakeClipLayer(kind, SKRect.Create(0, 0, 100, 100));
        clip.Children.Add(MakePicture(SKColors.Red, SKRect.Create(0, 0, 100, 100)));
        using var surface = SKSurface.Create(new SKImageInfo(Size.Width, Size.Height));
        var canvas = surface.Canvas;
        canvas.Save();
        var before = canvas.SaveCount;

        clip.Layout(LayerContext.Create(surface));

        Assert.Equal(before, canvas.SaveCount);
    }

    [Theory]
    [InlineData("rect")]
    [InlineData("rrect")]
    [InlineData("path")]
    public void Render_呼び出し元がクリップ済み_クリップの外へ描かない(string kind)
    {
        var clip = MakeClipLayer(kind, SKRect.Create(0, 0, 100, 100));
        clip.Children.Add(MakePicture(SKColors.Green, SKRect.Create(10, 10, 20, 20)));
        using var surface = SKSurface.Create(new SKImageInfo(Size.Width, Size.Height));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);
        canvas.Save();
        canvas.ClipRect(SKRect.Create(50, 0, 50, 100));
        var before = canvas.SaveCount;

        LayerRenderer.Render(clip, LayerContext.Create(surface));

        Assert.Equal(before, canvas.SaveCount);
        using var image = surface.Snapshot();
        Assert.Equal(SKColors.Empty, image.PeekPixels().GetPixelColor(15, 15));
    }
}
