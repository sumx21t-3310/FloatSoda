using System.Numerics;
using FloatSoda.Core;
using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.Painting;
using FloatSoda.RenderObjects;
using FloatSoda.Testing;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using SkiaSharp;

namespace FloatSoda.Test.Widgets;

public class ContainerTest
{
    private static readonly WidgetBitmapRenderer Renderer = new();
    private static readonly SKSizeI Size = new(100, 100);

    // WidgetBitmapRenderer のルートは無限の制約を渡すため、有限の制約が要るテストはこれで包む。
    private static SizedBox Bounded(Widget child) =>
        new()
        {
            Width = 100,
            Height = 100,
            Child = child,
        };

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
                    Child = new ColoredBox { Color = new Color(255, 0, 0) },
                },
            },
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
                    BorderRadius = BorderRadius.Circular(5),
                },
                Transform = Matrix3x2.CreateTranslation(25, 10),
            },
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
                Child = new ColoredBox { Color = new Color(255, 0, 0) },
            },
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
    public void Container_BorderありでPaddingを省略_子がボーダーの内側に収まる()
    {
        var widget = new Align
        {
            Alignment = Alignment.TopLeft,
            Child = new Container
            {
                Width = 60,
                Height = 60,
                Decoration = new BoxDecoration
                {
                    Border = Border.All(
                        new BorderSide { Color = new Color(0, 0, 255), Width = 10 }
                    ),
                },
                Child = new ColoredBox { Color = new Color(255, 0, 0) },
            },
        };

        using var bitmap = Renderer.Render(widget, Size);

        // ボーダー(各辺10)の上には子の赤が重ならず、子はボーダーの内側だけを占める。
        Assert.Equal(SKColors.Blue, bitmap.GetPixel(5, 5));
        Assert.Equal(SKColors.Red, bitmap.GetPixel(15, 15));
        Assert.Equal(SKColors.Red, bitmap.GetPixel(45, 45));
        Assert.Equal(SKColors.Blue, bitmap.GetPixel(55, 55));
    }

    [Fact]
    public void Container_PaddingとBorderを指定_余白とボーダーの太さの和だけ子が内側へ寄る()
    {
        var widget = new Align
        {
            Alignment = Alignment.TopLeft,
            Child = new Container
            {
                Width = 60,
                Height = 60,
                Decoration = new BoxDecoration
                {
                    Color = new Color(0, 255, 0),
                    Border = Border.All(new BorderSide { Color = new Color(0, 0, 255), Width = 5 }),
                },
                Padding = EdgeInsets.All(10),
                Child = new ColoredBox { Color = new Color(255, 0, 0) },
            },
        };

        using var bitmap = Renderer.Render(widget, Size);

        // ボーダー(0〜5)、余白(5〜15)、子(15〜45)の順に並ぶ。
        Assert.Equal(SKColors.Blue, bitmap.GetPixel(2, 2));
        Assert.Equal(SKColors.Lime, bitmap.GetPixel(12, 12));
        Assert.Equal(SKColors.Red, bitmap.GetPixel(16, 16));
        Assert.Equal(SKColors.Red, bitmap.GetPixel(44, 44));
        Assert.Equal(SKColors.Lime, bitmap.GetPixel(47, 47));
    }

    [Fact]
    public void Container_上辺だけにBorderを指定_上辺の太さだけ子が下へ寄る()
    {
        var widget = new Align
        {
            Alignment = Alignment.TopLeft,
            Child = new Container
            {
                Width = 60,
                Height = 60,
                Decoration = new BoxDecoration
                {
                    Border = new Border
                    {
                        Top = new BorderSide { Color = new Color(0, 0, 255), Width = 10 },
                    },
                },
                Child = new ColoredBox { Color = new Color(255, 0, 0) },
            },
        };

        using var bitmap = Renderer.Render(widget, Size);

        // ボーダーの無い左・右・下の辺には余白が入らず、子が端まで広がる。
        Assert.Equal(SKColors.Blue, bitmap.GetPixel(30, 5));
        Assert.Equal(SKColors.Red, bitmap.GetPixel(1, 15));
        Assert.Equal(SKColors.Red, bitmap.GetPixel(58, 58));
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
                Child = new ColoredBox { Color = new Color(255, 0, 0) },
            },
        };

        using var bitmap = Renderer.Render(widget, Size);

        // 余白ウィジェットが合成されないため、子の赤が背景の青を端まで覆う。
        Assert.Equal(SKColors.Red, bitmap.GetPixel(1, 1));
        Assert.Equal(SKColors.Red, bitmap.GetPixel(58, 58));
    }

    [Fact]
    public void Container_Childなしで緩い有限制約_最大サイズまで広がる()
    {
        var widget = Bounded(
            new Align
            {
                Alignment = Alignment.TopLeft,
                Child = new Container { Color = new Color(0, 0, 255) },
            }
        );

        using var bitmap = Renderer.Render(widget, Size);

        Assert.Equal(SKColors.Blue, bitmap.GetPixel(0, 0));
        Assert.Equal(SKColors.Blue, bitmap.GetPixel(99, 99));
    }

    [Fact]
    public void Container_Childなしで無限の制約_0になる()
    {
        // RenderView のルートは無限の制約を渡すため、ビューの大きさで両軸を確かめる。
        var view = new RenderView();
        var pipeline = new RenderPipeline { OnNeedVisualUpdate = () => { }, RenderView = view };
        view.PrepareInitialFrame();
        new RenderObjectToWidgetAdapter
        {
            Container = view,
            Child = new Container { Color = new Color(0, 0, 255) },
        }.AttachToRenderTree(new BuildOwner(() => { }), null);

        pipeline.FlushLayout();

        Assert.Equal(new SKSize(0, 0), view.Size);
    }

    [Fact]
    public void Container_ChildなしでPaddingを指定して無限の制約_余白の大きさになる()
    {
        var widget = Bounded(
            new OverflowBox
            {
                MinWidth = 0,
                MinHeight = 0,
                MaxWidth = double.PositiveInfinity,
                MaxHeight = double.PositiveInfinity,
                Child = new Container
                {
                    Color = new Color(0, 0, 255),
                    Padding = EdgeInsets.All(10),
                },
            }
        );

        using var bitmap = Renderer.Render(widget, Size);

        // 20x20 の領域が OverflowBox の中央(40〜60)に置かれる。
        Assert.Equal(SKColors.Blue, bitmap.GetPixel(41, 41));
        Assert.Equal(SKColors.Blue, bitmap.GetPixel(58, 58));
        Assert.Equal(0, bitmap.GetPixel(38, 38).Alpha);
        Assert.Equal(0, bitmap.GetPixel(61, 61).Alpha);
    }

    [Fact]
    public void Container_ChildなしでColumnの中_主軸の高さが0になる()
    {
        var widget = Bounded(
            new Column
            {
                Children =
                {
                    new Container { Color = new Color(0, 0, 255) },
                    new SizedBox
                    {
                        Width = 10,
                        Height = 10,
                        Child = new ColoredBox { Color = new Color(255, 0, 0) },
                    },
                },
            }
        );

        using var bitmap = Renderer.Render(widget, Size);

        // 主軸の高さが0なので、後ろの赤は上端から始まる。
        Assert.Equal(SKColors.Red, bitmap.GetPixel(50, 5));
    }

    [Fact]
    public void Container_ChildなしでWidthとHeightを指定_指定サイズになる()
    {
        var widget = new Align
        {
            Alignment = Alignment.TopLeft,
            Child = new Container
            {
                Width = 40,
                Height = 30,
                Color = new Color(0, 0, 255),
            },
        };

        using var bitmap = Renderer.Render(widget, Size);

        Assert.Equal(SKColors.Blue, bitmap.GetPixel(39, 29));
        Assert.Equal(0, bitmap.GetPixel(41, 29).Alpha);
        Assert.Equal(0, bitmap.GetPixel(39, 31).Alpha);
    }

    [Fact]
    public void Container_ChildなしでWidthだけを指定_幅は指定値で高さは最大まで広がる()
    {
        var widget = Bounded(
            new Align
            {
                Alignment = Alignment.TopLeft,
                Child = new Container { Width = 40, Color = new Color(0, 0, 255) },
            }
        );

        using var bitmap = Renderer.Render(widget, Size);

        Assert.Equal(SKColors.Blue, bitmap.GetPixel(39, 99));
        Assert.Equal(0, bitmap.GetPixel(41, 50).Alpha);
    }

    [Fact]
    public void Container_ChildなしでAlignmentを指定_Alignmentを無視して最大サイズまで広がる()
    {
        var widget = Bounded(
            new Align
            {
                Alignment = Alignment.TopLeft,
                Child = new Container
                {
                    Alignment = Alignment.BottomRight,
                    Color = new Color(0, 0, 255),
                },
            }
        );

        using var bitmap = Renderer.Render(widget, Size);

        Assert.Equal(SKColors.Blue, bitmap.GetPixel(0, 0));
        Assert.Equal(SKColors.Blue, bitmap.GetPixel(99, 99));
    }

    [Fact]
    public void Container_ChildなしでIntrinsicWidthの中_幅が0になる()
    {
        var widget = Bounded(
            new Align
            {
                Alignment = Alignment.TopLeft,
                Child = new IntrinsicWidth
                {
                    Child = new Container { Color = new Color(0, 0, 255) },
                },
            }
        );

        using var bitmap = Renderer.Render(widget, Size);

        Assert.Equal(0, bitmap.GetPixel(0, 50).Alpha);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Container_Paddingが負数または非有限値_ArgumentOutOfRangeExceptionを投げる(
        double value
    )
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Container { Padding = EdgeInsets.All(value) }
        );
    }

    [Fact]
    public void Container_ColorとDecorationを同時に指定_InvalidOperationExceptionを投げる()
    {
        var container = new Container
        {
            Color = new Color(0, 0, 255),
            Decoration = new BoxDecoration { Color = new Color(255, 0, 0) },
        };

        Assert.Throws<InvalidOperationException>(() => Renderer.Render(container, Size));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Container_WidthとHeightが負数または非有限値_ArgumentOutOfRangeExceptionを投げる(
        double value
    )
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Container { Width = value });
        Assert.Throws<ArgumentOutOfRangeException>(() => new Container { Height = value });
    }
}
