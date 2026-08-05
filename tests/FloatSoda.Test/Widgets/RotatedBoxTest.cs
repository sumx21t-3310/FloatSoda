using FloatSoda.Abstractions.Geometries;
using FloatSoda.Geometrics;
using FloatSoda.Gesture;
using FloatSoda.RenderObjects.Layout;
using FloatSoda.RenderObjects.Painting;
using FloatSoda.Testing;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using SkiaSharp;

namespace FloatSoda.Test.Widgets;

public class RotatedBoxTest
{
    private static readonly WidgetBitmapRenderer Renderer = new();
    private static readonly SKSizeI Size = new(100, 100);

    [Theory]
    [InlineData(0, 40, 20)]
    [InlineData(1, 20, 40)]
    [InlineData(2, 40, 20)]
    [InlineData(3, 20, 40)]
    public void PerformLayout_QuarterTurnsに応じて非正方形の寸法を回転する(int quarterTurns, int width, int height)
    {
        var child = new RenderConstrainedBox { AdditionalConstraints = BoxConstraints.Tight(40, 20) };
        var rotated = new RenderRotatedBox { QuarterTurns = quarterTurns, Child = child };

        rotated.Layout(BoxConstraints.Loose(100, 100));

        Assert.Equal(new SKSize(width, height), rotated.Size);
    }

    [Theory]
    [InlineData(-1, 3)]
    [InlineData(5, 1)]
    [InlineData(1_000_002, 2)]
    [InlineData(int.MinValue, 0)]
    public void QuarterTurns_負値または大値_4を法として正規化する(int input, int expected)
    {
        var rotated = new RenderRotatedBox { QuarterTurns = input };
        Assert.Equal(expected, rotated.QuarterTurns);
    }

    [Theory]
    [InlineData(0, 35, 50, 65, 50, 255, 0, 0, 0, 0, 255)]
    [InlineData(1, 50, 35, 50, 65, 255, 0, 0, 0, 0, 255)]
    [InlineData(2, 35, 50, 65, 50, 0, 0, 255, 255, 0, 0)]
    [InlineData(3, 50, 35, 50, 65, 0, 0, 255, 255, 0, 0)]
    public void RotatedBox_0度から270度_期待する向きで描画する(
        int quarterTurns,
        int firstX,
        int firstY,
        int secondX,
        int secondY,
        byte firstRed,
        byte firstGreen,
        byte firstBlue,
        byte secondRed,
        byte secondGreen,
        byte secondBlue)
    {
        using var bitmap = Renderer.Render(BuildPattern(quarterTurns), Size);

        Assert.Equal(new SKColor(firstRed, firstGreen, firstBlue), bitmap.GetPixel(firstX, firstY));
        Assert.Equal(new SKColor(secondRed, secondGreen, secondBlue), bitmap.GetPixel(secondX, secondY));
        Assert.Equal(0, bitmap.GetPixel(10, 10).Alpha);
    }

    [Theory]
    [InlineData(0, 30, 10, 10, 30)]
    [InlineData(1, 10, 30, 30, 10)]
    [InlineData(2, 30, 10, 10, 30)]
    [InlineData(3, 10, 30, 30, 10)]
    public void HitTestChildren_0度から270度_回転後の領域だけがヒットする(
        int quarterTurns,
        int insideX,
        int insideY,
        int outsideX,
        int outsideY)
    {
        var rotated = new RenderRotatedBox
        {
            QuarterTurns = quarterTurns,
            Child = new RenderColoredBox
            {
                Child = new RenderConstrainedBox { AdditionalConstraints = BoxConstraints.Tight(40, 20) }
            }
        };
        rotated.Layout(BoxConstraints.Loose(100, 100));

        Assert.True(rotated.HitTest(new HitTestResult(), new Offset(insideX, insideY)));
        Assert.False(rotated.HitTest(new HitTestResult(), new Offset(outsideX, outsideY)));
    }

    [Fact]
    public void QuarterTurns_更新時_LayoutDirtyにして寸法を更新する()
    {
        var rotated = new RenderRotatedBox
        {
            QuarterTurns = 0,
            Child = new RenderConstrainedBox { AdditionalConstraints = BoxConstraints.Tight(40, 20) }
        };
        rotated.Layout(BoxConstraints.Loose(100, 100));

        rotated.QuarterTurns = 1;

        Assert.True(rotated.NeedsLayout);
        rotated.Layout(BoxConstraints.Loose(100, 100));
        Assert.Equal(new SKSize(20, 40), rotated.Size);
    }

    private static SizedBox BuildPattern(int quarterTurns) => new()
    {
        Width = Size.Width,
        Height = Size.Height,
        Child = new Align
        {
            Alignment = Alignment.Center,
            Child = new RotatedBox
            {
                QuarterTurns = quarterTurns,
                Child = new Row
                {
                    MainAxisSize = MainAxisSize.Min,
                    Children =
                    [
                        new SizedBox
                        {
                            Width = 20,
                            Height = 20,
                            Child = new ColoredBox { Color = new Color(255, 0, 0) }
                        },
                        new SizedBox
                        {
                            Width = 20,
                            Height = 20,
                            Child = new ColoredBox { Color = new Color(0, 0, 255) }
                        }
                    ]
                }
            }
        }
    };
}
