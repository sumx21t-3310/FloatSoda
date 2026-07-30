using System.Numerics;
using FloatSoda.Geometrics;
using FloatSoda.Painting;
using FloatSoda.Testing;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using SkiaSharp;

namespace FloatSoda.Test.Widgets;

public class ContainerTest
{
    private static readonly WidgetBitmapRenderer Renderer = new();
    private static readonly SKSizeI Size = new(100, 100);

    [Fact]
    public void ComposesSizeColorAndAlignment()
    {
        var widget = new Align
        {
            Alignment = Alignment.TopLeft,
            Child = new Container
            {
                Width = 40,
                Height = 30,
                Color = new Color(0, 0, 255),
                Alignment = Alignment.BottomRight,
                Child = new SizedBox
                {
                    Width = 10,
                    Height = 10,
                    Child = new ColoredBox { Color = new Color(255, 0, 0) }
                }
            }
        };

        using var bitmap = Renderer.Render(widget, Size);

        Assert.Equal(SKColors.Blue, bitmap.GetPixel(5, 5));
        Assert.Equal(SKColors.Red, bitmap.GetPixel(35, 25));
        Assert.Equal(0, bitmap.GetPixel(50, 50).Alpha);
    }

    [Fact]
    public void ComposesDecorationAndTransform()
    {
        var widget = new Align
        {
            Alignment = Alignment.TopLeft,
            Child = new Container
            {
                Width = 20,
                Height = 20,
                Decoration = new BoxDecoration
                {
                    Color = new Color(0, 0, 255),
                    BorderRadius = BorderRadius.Circular(5)
                },
                Transform = Matrix3x2.CreateTranslation(25, 10)
            }
        };

        using var bitmap = Renderer.Render(widget, Size);

        Assert.Equal(0, bitmap.GetPixel(5, 5).Alpha);
        Assert.Equal(SKColors.Blue, bitmap.GetPixel(35, 20));
    }

    [Fact]
    public void RejectsColorTogetherWithDecoration()
    {
        var container = new Container
        {
            Color = new Color(0, 0, 255),
            Decoration = new BoxDecoration { Color = new Color(255, 0, 0) }
        };

        Assert.Throws<InvalidOperationException>(() => Renderer.Render(container, Size));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void RejectsInvalidDimensions(double value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Container { Width = value });
        Assert.Throws<ArgumentOutOfRangeException>(() => new Container { Height = value });
    }
}
