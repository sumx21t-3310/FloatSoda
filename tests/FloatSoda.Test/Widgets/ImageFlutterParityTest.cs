using FloatSoda.Geometrics;
using FloatSoda.Geometry;
using FloatSoda.Gesture;
using FloatSoda.RenderObjects.Layout;
using FloatSoda.RenderObjects.Painting;
using FloatSoda.Testing;
using SkiaSharp;

namespace FloatSoda.Test.Widgets;

public class ImageFlutterParityTest
{
    [Fact]
    public void HitTest_画像の領域内_自身をヒット対象にする()
    {
        using var bitmap = new SKBitmap(8, 8);
        using var image = SKImage.FromBitmap(bitmap);
        var renderImage = new RenderImage { Image = image };
        renderImage.Layout(BoxConstraints.Tight(8, 8));
        var result = new HitTestResult();

        var hit = renderImage.HitTest(result, new Offset(4, 4));

        Assert.True(hit);
        Assert.Contains(result.Path, entry => ReferenceEquals(entry.Target, renderImage));
    }

    [Fact]
    public void Paint_Fitを省略_画像を原寸より拡大しない()
    {
        using var sourceBitmap = new SKBitmap(8, 4);
        sourceBitmap.Erase(SKColors.Blue);
        using var image = SKImage.FromBitmap(sourceBitmap);
        var renderImage = new RenderImage { Image = image };
        using var rendered = new RenderObjectBitmapRenderer().Render(
            new RenderConstrainedBox
            {
                AdditionalConstraints = BoxConstraints.Tight(40, 40),
                Child = renderImage,
            },
            new SKSizeI(40, 40)
        );

        Assert.Equal(default, rendered.GetPixel(2, 15));
    }
}
