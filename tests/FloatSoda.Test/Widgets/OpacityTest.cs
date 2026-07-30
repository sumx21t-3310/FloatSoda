using FloatSoda.Geometrics;
using FloatSoda.Testing;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using SkiaSharp;

namespace FloatSoda.Test.Widgets;

public class OpacityTest
{
    private static readonly WidgetBitmapRenderer Renderer = new();
    private static readonly SKSizeI Size = new(40, 40);

    [Fact]
    public void PaintsChildWithRequestedOpacity()
    {
        var widget = new SizedBox
        {
            Width = Size.Width,
            Height = Size.Height,
            Child = new Opacity
            {
                Value = 0.5,
                Child = new ColoredBox { Color = new Color(255, 0, 0) }
            }
        };

        using var bitmap = Renderer.Render(widget, Size);

        var pixel = bitmap.GetPixel(20, 20);
        Assert.Equal(255, pixel.Red);
        Assert.InRange(pixel.Alpha, 127, 128);
    }

    [Fact]
    public void ZeroSkipsChildPainting()
    {
        var widget = new SizedBox
        {
            Width = Size.Width,
            Height = Size.Height,
            Child = new Opacity
            {
                Value = 0,
                Child = new ColoredBox { Color = new Color(255, 0, 0) }
            }
        };

        using var bitmap = Renderer.Render(widget, Size);

        Assert.Equal(0, bitmap.GetPixel(20, 20).Alpha);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void RejectsInvalidOpacity(double value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Opacity { Value = value });
    }
}
