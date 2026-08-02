using FloatSoda.Geometrics;
using FloatSoda.Painting;
using FloatSoda.Testing;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using SkiaSharp;

namespace FloatSoda.Test.Widgets;

public class DecoratedBoxTest
{
    private static readonly WidgetBitmapRenderer Renderer = new();
    private static readonly SKSizeI Size = new(60, 60);

    [Fact]
    public void DecoratedBox_背景色とボーダーを指定_外周がボーダー色で内側が背景色になる()
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
    public void DecoratedBox_BorderRadiusを指定_角丸の外側は描画されない()
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
    public void DecoratedBox_PositionがForeground_子の前面へ装飾が描画される()
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
    public void DecoratedBox_ボーダーがボックスの短辺より太い_装飾の外形からはみ出さない()
    {
        var widget = new Align
        {
            Alignment = Alignment.TopLeft,
            Child = new SizedBox
            {
                Width = 10,
                Height = 10,
                Child = new DecoratedBox
                {
                    Decoration = new BoxDecoration
                    {
                        Border = Border.All(new BorderSide
                        {
                            Color = new Color(255, 0, 0),
                            Width = 100
                        })
                    }
                }
            }
        };

        using var bitmap = Renderer.Render(widget, Size);

        Assert.Equal(SKColors.Red, bitmap.GetPixel(5, 5));
        Assert.Equal(0, bitmap.GetPixel(11, 5).Alpha);
        Assert.Equal(0, bitmap.GetPixel(5, 11).Alpha);
        Assert.Equal(0, bitmap.GetPixel(30, 30).Alpha);
    }

    [Fact]
    public void BorderSide_Widthが負数または非有限値_ArgumentOutOfRangeExceptionを投げる()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new BorderSide { Width = -1 });
        Assert.Throws<ArgumentOutOfRangeException>(() => new BorderSide { Width = double.NaN });
        Assert.Throws<ArgumentOutOfRangeException>(() => new BorderSide { Width = double.PositiveInfinity });
    }

    [Fact]
    public void BoxDecoration_BorderRadiusが負数_ArgumentOutOfRangeExceptionを投げる()
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
