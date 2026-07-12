using FloatSoda.Common.Layer;
using FloatSoda.Rendering;
using SkiaSharp;

namespace FloatSoda.Common.Test.Layers;

public class LayerTest
{
    private static readonly ImageRenderer Renderer = new();
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

        using var bitmap = Renderer.RenderLayerTree(layer, Size);

        Assert.Equal(SKColors.Red, bitmap.GetPixel(50, 50));
    }

    [Fact]
    public void ContainerLayer_PaintsAllChildren()
    {
        var container = new ContainerLayer();
        container.Children.Add(MakePicture(SKColors.Red, SKRect.Create(0, 0, 50, 100)));
        container.Children.Add(MakePicture(SKColors.Blue, SKRect.Create(50, 0, 50, 100)));

        using var bitmap = Renderer.RenderLayerTree(container, Size);

        Assert.Equal(SKColors.Red, bitmap.GetPixel(25, 50));
        Assert.Equal(SKColors.Blue, bitmap.GetPixel(75, 50));
    }

    [Fact]
    public void ClipRectLayer_ClipsToRect()
    {
        var clip = new ClipRectLayer(SKRect.Create(0, 0, 50, 100)) { ClipBehavior = Clip.HardEdge };
        clip.Children.Add(MakePicture(SKColors.Red, SKRect.Create(0, 0, 100, 100)));

        using var bitmap = Renderer.RenderLayerTree(clip, Size);

        Assert.Equal(SKColors.Red, bitmap.GetPixel(25, 50));
        Assert.Equal(SKColors.Empty, bitmap.GetPixel(75, 50));
    }

    [Fact]
    public void ClipRoundRectLayer_ClipsCorners()
    {
        var roundRect = new SKRoundRect(SKRect.Create(0, 0, 100, 100), 40, 40);
        var clip = new ClipRoundRectLayer(roundRect) { ClipBehavior = Clip.HardEdge };
        clip.Children.Add(MakePicture(SKColors.Red, SKRect.Create(0, 0, 100, 100)));

        using var bitmap = Renderer.RenderLayerTree(clip, Size);

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

        using var bitmap = Renderer.RenderLayerTree(clip, Size);

        Assert.Equal(SKColors.Red, bitmap.GetPixel(25, 50));
        Assert.Equal(SKColors.Empty, bitmap.GetPixel(75, 50));
    }

    [Fact]
    public void OpacityLayer_AppliesAlpha()
    {
        var opacity = new OpacityLayer { Alpha = 128 };
        opacity.Children.Add(MakePicture(SKColors.Red, SKRect.Create(0, 0, 100, 100)));

        using var bitmap = Renderer.RenderLayerTree(opacity, Size);
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

        using var bitmap = Renderer.RenderLayerTree(transform, Size);

        Assert.Equal(SKColors.Blue, bitmap.GetPixel(55, 55));
        Assert.Equal(SKColors.Empty, bitmap.GetPixel(5, 5));
    }
}
