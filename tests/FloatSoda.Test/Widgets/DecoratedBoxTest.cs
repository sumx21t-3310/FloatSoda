using FloatSoda.Geometrics;
using FloatSoda.Painting;
using FloatSoda.Testing;
using FloatSoda.Widgets.Paint;
using SkiaSharp;

namespace FloatSoda.Test.Widgets;

public class DecoratedBoxTest
{
    private static readonly WidgetBitmapRenderer Renderer = new();
    private static readonly SKSizeI Size = new(60, 60);

    [Fact]
    public void PaintsBackgroundColorAndBorder()
    {
        var widget = Fill(new DecoratedBox
        {
            Decoration = new BoxDecoration
            {
                Color = new Color(0, 0, 255),
                Border = Border.All(new BorderSide
                {
                    Color = new Color(255, 0, 0),
                    Width = 4
                })
            }
        });

        using var bitmap = Renderer.Render(widget, Size);

        Assert.Equal(SKColors.Red, bitmap.GetPixel(1, 30));
        Assert.Equal(SKColors.Blue, bitmap.GetPixel(30, 30));
    }

    [Fact]
    public void RoundedCornersDoNotPaintOutsideShape()
    {
        var widget = Fill(new DecoratedBox
        {
            Decoration = new BoxDecoration
            {
                Color = new Color(0, 0, 255),
                BorderRadius = BorderRadius.Circular(20)
            }
        });

        using var bitmap = Renderer.Render(widget, Size);

        Assert.Equal(0, bitmap.GetPixel(0, 0).Alpha);
        Assert.Equal(SKColors.Blue, bitmap.GetPixel(30, 30));
    }

    [Fact]
    public void ForegroundDecorationPaintsOverChild()
    {
        var widget = Fill(new DecoratedBox
        {
            Position = DecorationPosition.Foreground,
            Decoration = new BoxDecoration { Color = new Color(255, 0, 0) },
            Child = new ColoredBox { Color = new Color(0, 0, 255) }
        });

        using var bitmap = Renderer.Render(widget, Size);

        Assert.Equal(SKColors.Red, bitmap.GetPixel(30, 30));
    }

    [Fact]
    public void RejectsNegativeAndNonFiniteBorderWidths()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new BorderSide { Width = -1 });
        Assert.Throws<ArgumentOutOfRangeException>(() => new BorderSide { Width = double.NaN });
        Assert.Throws<ArgumentOutOfRangeException>(() => new BorderSide { Width = double.PositiveInfinity });
    }

    [Fact]
    public void RejectsInvalidCornerRadius()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new BoxDecoration
        {
            BorderRadius = BorderRadius.Circular(-1)
        });
    }

    private static FloatSoda.Widgets.Widget Fill(FloatSoda.Widgets.Widget child) => new FloatSoda.Widgets.Layout.SizedBox
    {
        Width = Size.Width,
        Height = Size.Height,
        Child = child
    };
}
