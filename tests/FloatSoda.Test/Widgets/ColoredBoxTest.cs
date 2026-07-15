using FloatSoda.Testing;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using SkiaSharp;

namespace FloatSoda.Test.Widgets;

public class ColoredBoxTest
{
    private static readonly WidgetBitmapRenderer Renderer = new();
    private static readonly SKSizeI Size = new(100, 100);

    [Fact]
    public void PaintsCorrectColor()
    {
        var widget = new SizedBox
        {
            Width = Size.Width,
            Height = Size.Height,
            Child = new ColoredBox { Color = SKColors.Blue }
        };

        using var bitmap = Renderer.Render(widget, Size);

        Assert.Equal(SKColors.Blue, bitmap.GetPixel(50, 50));
    }
}
