using FloatSoda.Geometrics;
using FloatSoda.Testing;
using FloatSoda.RenderObjects.Layout;
using FloatSoda.RenderObjects.Painting;
using SkiaSharp;

namespace FloatSoda.Test.RenderObjects;

public class RenderColoredBoxTest
{
    private static readonly RenderObjectBitmapRenderer Renderer = new();
    private static readonly SKSizeI Size = new(100, 100);

    [Fact]
    public void PaintsCorrectColor()
    {
        var renderBox = new RenderConstrainedBox
        {
            AdditionalConstraints = BoxConstraints.Tight(Size.Width, Size.Height),
            Child = new RenderColoredBox { Color = SKColors.Red }
        };

        using var bitmap = Renderer.Render(renderBox, Size);

        Assert.Equal(SKColors.Red, bitmap.GetPixel(50, 50));
    }
}
