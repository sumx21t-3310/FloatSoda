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
    public void Container_WidthとHeightとColorとAlignmentを指定_指定サイズの色付き領域に子が配置される()
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
    public void Container_DecorationとTransformを指定_装飾が変換後の位置へ描画される()
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
    public void Container_PaddingとDecorationを指定_装飾の内側へ余白が入る()
    {
        var widget = new Align
        {
            Alignment = Alignment.TopLeft,
            Child = new Container
            {
                Width = 60,
                Height = 60,
                Decoration = new BoxDecoration { Color = new Color(0, 0, 255) },
                Padding = EdgeInsets.All(10),
                Child = new ColoredBox { Color = new Color(255, 0, 0) }
            }
        };

        using var bitmap = Renderer.Render(widget, Size);

        // 余白領域(各辺10)は装飾の青で塗られ、子の赤は余白の内側だけを占める。
        Assert.Equal(SKColors.Blue, bitmap.GetPixel(5, 5));
        Assert.Equal(SKColors.Red, bitmap.GetPixel(15, 15));
        Assert.Equal(SKColors.Red, bitmap.GetPixel(45, 45));
        Assert.Equal(SKColors.Blue, bitmap.GetPixel(55, 55));
        Assert.Equal(0, bitmap.GetPixel(65, 65).Alpha);
    }

    [Fact]
    public void Container_Paddingがnull_余白なしで子が全域へ描画される()
    {
        var widget = new Align
        {
            Alignment = Alignment.TopLeft,
            Child = new Container
            {
                Width = 60,
                Height = 60,
                Color = new Color(0, 0, 255),
                Child = new ColoredBox { Color = new Color(255, 0, 0) }
            }
        };

        using var bitmap = Renderer.Render(widget, Size);

        // 余白ウィジェットが合成されないため、子の赤が背景の青を端まで覆う。
        Assert.Equal(SKColors.Red, bitmap.GetPixel(1, 1));
        Assert.Equal(SKColors.Red, bitmap.GetPixel(58, 58));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Container_Paddingが負数または非有限値_ArgumentOutOfRangeExceptionを投げる(double value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Container { Padding = EdgeInsets.All(value) });
    }

    [Fact]
    public void Container_ColorとDecorationを同時に指定_InvalidOperationExceptionを投げる()
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
    public void Container_WidthとHeightが負数または非有限値_ArgumentOutOfRangeExceptionを投げる(double value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Container { Width = value });
        Assert.Throws<ArgumentOutOfRangeException>(() => new Container { Height = value });
    }
}
