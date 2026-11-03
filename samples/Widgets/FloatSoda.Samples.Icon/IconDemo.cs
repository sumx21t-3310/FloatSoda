using FloatSoda.Core;
using FloatSoda.Core.Providers;
using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.Painting;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using IconWidget = FloatSoda.Widgets.Paint.Icon;

namespace FloatSoda.Samples.Icon;

/// <summary>アイコンフォントの字を、大きさと色を変えて表示するサンプルです。</summary>
public sealed record IconDemo : StatelessWidget
{
    // Windows 10 以降に入っているアイコンフォント。字はコードポイントで指定する。
    private static readonly FontProvider IconFont = new SystemFontProvider("Segoe MDL2 Assets");

    private static readonly IconData Home = new(0xE80F, IconFont);
    private static readonly IconData Settings = new(0xE713, IconFont);
    private static readonly IconData Search = new(0xE721, IconFont);
    private static readonly IconData Star = new(0xE734, IconFont);

    /// <inheritdoc />
    public override Widget Build(IBuildContext context) =>
        new SizedBox
        {
            Width = 820,
            Height = 520,
            Child = new ColoredBox
            {
                Color = new Color(16, 20, 31),
                Child = new Padding
                {
                    Spacing = EdgeInsets.All(32),
                    Child = new Column
                    {
                        CrossAxisAlignment = CrossAxisAlignment.Start,
                        Children =
                        {
                            Label("Color を省略すると黒で描く(明るい背景の上に置く)"),
                            new SizedBox { Height = 10 },
                            new ColoredBox
                            {
                                Color = new Color(226, 232, 240),
                                Child = new Padding
                                {
                                    Spacing = EdgeInsets.All(8),
                                    Child = new IconWidget(Home),
                                },
                            },
                            new SizedBox { Height = 28 },
                            Label("Size — 正方形の一辺。字の大きさも同じ値になる"),
                            new SizedBox { Height = 10 },
                            new Row
                            {
                                Children =
                                {
                                    // 灰色の背景は Icon の領域(Size × Size)を示す。
                                    Frame(
                                        new IconWidget(Star)
                                        {
                                            Size = 24,
                                            Color = new Color(255, 209, 102),
                                        }
                                    ),
                                    new SizedBox { Width = 24 },
                                    Frame(
                                        new IconWidget(Star)
                                        {
                                            Size = 48,
                                            Color = new Color(255, 209, 102),
                                        }
                                    ),
                                    new SizedBox { Width = 24 },
                                    Frame(
                                        new IconWidget(Star)
                                        {
                                            Size = 96,
                                            Color = new Color(255, 209, 102),
                                        }
                                    ),
                                },
                            },
                            new SizedBox { Height = 28 },
                            Label("Color — 字の色を変える"),
                            new SizedBox { Height = 10 },
                            new Row
                            {
                                Children =
                                {
                                    new IconWidget(Home)
                                    {
                                        Size = 48,
                                        Color = new Color(124, 205, 255),
                                    },
                                    new SizedBox { Width = 24 },
                                    new IconWidget(Settings)
                                    {
                                        Size = 48,
                                        Color = new Color(255, 111, 97),
                                    },
                                    new SizedBox { Width = 24 },
                                    new IconWidget(Search)
                                    {
                                        Size = 48,
                                        Color = new Color(244, 247, 255),
                                    },
                                },
                            },
                            new SizedBox { Height = 28 },
                            Label("文字と並べる"),
                            new SizedBox { Height = 10 },
                            new Row
                            {
                                Children =
                                {
                                    new IconWidget(Settings)
                                    {
                                        Size = 32,
                                        Color = new Color(244, 247, 255),
                                    },
                                    new SizedBox { Width = 12 },
                                    new Text("設定")
                                    {
                                        Style = new TextStyle
                                        {
                                            FontSize = 28,
                                            Color = new Color(244, 247, 255),
                                        },
                                    },
                                },
                            },
                        },
                    },
                },
            },
        };

    private static Widget Frame(Widget child) =>
        new ColoredBox { Color = new Color(40, 47, 64), Child = child };

    private static Widget Label(string text) =>
        new Text(text)
        {
            Style = new TextStyle { FontSize = 18, Color = new Color(169, 180, 204) },
        };
}
