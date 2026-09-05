using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.Painting;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using AspectRatioWidget = FloatSoda.Widgets.Layout.AspectRatio;

namespace FloatSoda.Samples.AspectRatio;

/// <summary>比率ごとの寸法の決まり方と、tight 制約下では比率が効かないことを確認するサンプルです。</summary>
public sealed record AspectRatioDemo : StatelessWidget
{
    /// <inheritdoc />
    public override Widget Build(IBuildContext context) => new SizedBox
    {
        Width = 760,
        Height = 280,
        Child = new ColoredBox
        {
            Color = new Color(16, 20, 31),
            Child = new Padding
            {
                Spacing = EdgeInsets.All(32),
                Child = new Row
                {
                    Children =
                    {
                        // 幅いっぱい(150)を採り、高さを 150 / (16/9) ≈ 84 に決める。
                        Cell("16 : 9", new Center
                        {
                            Child = new AspectRatioWidget
                            {
                                Ratio = 16.0 / 9.0,
                                Child = Fill(new Color(124, 205, 255))
                            }
                        }),
                        new SizedBox { Width = 24 },

                        // 高さが先に上限へ達する例。幅 150 では高さ 200 が要るため、
                        // 高さ 150 から逆算して幅 112.5 になる。
                        Cell("3 : 4", new Center
                        {
                            Child = new AspectRatioWidget
                            {
                                Ratio = 3.0 / 4.0,
                                Child = Fill(new Color(255, 111, 97))
                            }
                        }),
                        new SizedBox { Width = 24 },

                        Cell("1 : 2", new Center
                        {
                            Child = new AspectRatioWidget
                            {
                                Ratio = 1.0 / 2.0,
                                Child = Fill(new Color(255, 209, 102))
                            }
                        }),
                        new SizedBox { Width = 24 },

                        // 親が寸法を固定(tight)していると比率は効かず、親の寸法になる。
                        // 上の3つが Center を挟んで緩い制約を作っているのはこのため。
                        Cell("tight では効かない", new AspectRatioWidget
                        {
                            Ratio = 16.0 / 9.0,
                            Child = Fill(new Color(124, 205, 255))
                        })
                    }
                }
            }
        }
    };

    /// <summary>ラベル付きの枠を作り、その中で AspectRatio の効果を見せる。</summary>
    private static Widget Cell(string label, Widget child) => new Column
    {
        MainAxisSize = MainAxisSize.Min,
        Children =
        {
            new ColoredBox
            {
                Color = new Color(40, 47, 64),
                Child = new SizedBox
                {
                    Width = 150,
                    Height = 150,
                    Child = child
                }
            },
            new SizedBox { Height = 10 },
            new Text(label)
            {
                Style = new TextStyle { FontSize = 18, Color = new Color(169, 180, 204) }
            }
        }
    };

    /// <summary>与えられた領域いっぱいに広がる塗り。AspectRatio 自身の範囲を可視化する。</summary>
    private static Widget Fill(Color color) => new ColoredBox
    {
        Color = color,
        Child = new SizedBox()
    };
}
