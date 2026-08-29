using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.Painting;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using WrapWidget = FloatSoda.Widgets.Layout.Wrap;

namespace FloatSoda.Samples.Wrap;

/// <summary>折り返しと間隔、run 内の主軸・交差軸の揃え方を確認するサンプルです。</summary>
public sealed record WrapDemo : StatelessWidget
{
    /// <inheritdoc />
    public override Widget Build(IBuildContext context) => new SizedBox
    {
        Width = 560,
        Height = 580,
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
                        // 幅360に収まらない子は次の run へ折り返される。
                        // Spacing が子同士の間隔、RunSpacing が run 同士の間隔。
                        Label("折り返しと間隔 — Spacing / RunSpacing"),
                        new SizedBox { Height = 10 },
                        Frame(new WrapWidget
                        {
                            Spacing = 8,
                            RunSpacing = 8,
                            Children =
                            {
                                Chip(new Color(124, 205, 255), 90),
                                Chip(new Color(255, 111, 97), 60),
                                Chip(new Color(255, 209, 102), 120),
                                Chip(new Color(124, 205, 255), 70),
                                Chip(new Color(255, 111, 97), 100),
                                Chip(new Color(255, 209, 102), 80),
                                Chip(new Color(124, 205, 255), 110),
                                Chip(new Color(255, 111, 97), 50)
                            }
                        }),
                        new SizedBox { Height = 24 },

                        // Alignment は run ごとの主軸方向の余りの配り方。
                        Label("Alignment.Center — run 内の余りを中央へ"),
                        new SizedBox { Height = 10 },
                        Frame(new WrapWidget
                        {
                            Alignment = WrapAlignment.Center,
                            Spacing = 8,
                            RunSpacing = 8,
                            Children =
                            {
                                Chip(new Color(124, 205, 255), 90),
                                Chip(new Color(255, 111, 97), 60),
                                Chip(new Color(255, 209, 102), 120),
                                Chip(new Color(124, 205, 255), 70),
                                Chip(new Color(255, 111, 97), 100),
                                Chip(new Color(255, 209, 102), 80),
                                Chip(new Color(124, 205, 255), 110),
                                Chip(new Color(255, 111, 97), 50)
                            }
                        }),
                        new SizedBox { Height = 24 },

                        // CrossAxisAlignment は run 内での交差軸(ここでは高さ)の揃え方。
                        Label("CrossAxisAlignment.Center — 高さ違いを中央で揃える"),
                        new SizedBox { Height = 10 },
                        Frame(new WrapWidget
                        {
                            CrossAxisAlignment = WrapCrossAlignment.Center,
                            Spacing = 8,
                            RunSpacing = 8,
                            Children =
                            {
                                Box(new Color(124, 205, 255), 80, 32),
                                Box(new Color(255, 111, 97), 80, 48),
                                Box(new Color(255, 209, 102), 80, 64),
                                Box(new Color(124, 205, 255), 80, 32),
                                Box(new Color(255, 111, 97), 80, 48)
                            }
                        })
                    }
                }
            }
        }
    };

    /// <summary>幅360の枠。この幅を超えた子が折り返される。</summary>
    private static Widget Frame(Widget child) => new ColoredBox
    {
        Color = new Color(40, 47, 64),
        Child = new SizedBox
        {
            Width = 360,
            Child = child
        }
    };

    private static Widget Label(string text) => new Text(text)
    {
        Style = new TextStyle { FontSize = 18, Color = new Color(169, 180, 204) }
    };

    /// <summary>高さ32で幅だけ異なるチップ。折り返し位置を作る。</summary>
    private static Widget Chip(Color color, double width) => Box(color, width, 32);

    private static Widget Box(Color color, double width, double height) => new ColoredBox
    {
        Color = color,
        Child = new SizedBox { Width = width, Height = height }
    };
}
