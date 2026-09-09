using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.Painting;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using OverflowBoxWidget = FloatSoda.Widgets.Layout.OverflowBox;

namespace FloatSoda.Samples.OverflowBox;

/// <summary>親と無関係な制約の受け渡しと、自身の寸法の決め方(Fit / SizedOverflowBox)を確認するサンプルです。</summary>
public sealed record OverflowBoxDemo : StatelessWidget
{
    /// <inheritdoc />
    public override Widget Build(IBuildContext context) => new SizedBox
    {
        Width = 800,
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
                        // 自身は枠の寸法のまま、子には枠より大きい最大幅を渡す。
                        // 幅190の子は枠(150)からはみ出し、クリップされずそのまま描かれる。
                        Cell("親より大きい制約", new OverflowBoxWidget
                        {
                            MinHeight = 0,
                            MaxWidth = 210,
                            Child = Bar(new Color(124, 205, 255), 190, 40)
                        }),
                        new SizedBox { Width = 24 },

                        // Fit の既定は Max。自身は使える領域いっぱい(150)に広がる。
                        // 茶色が OverflowBox 自身の範囲。
                        Cell("Fit = Max(既定)", new Center
                        {
                            Child = new ColoredBox
                            {
                                Color = new Color(92, 76, 44),
                                Child = new OverflowBoxWidget
                                {
                                    Child = Bar(new Color(255, 111, 97), 60, 60)
                                }
                            }
                        }),
                        new SizedBox { Width = 24 },

                        // DeferToChild にすると、自身は子と同じ寸法まで縮む。
                        // 茶色が子の背後にぴったり隠れ、枠の背景色が見える。
                        Cell("Fit = DeferToChild", new Center
                        {
                            Child = new ColoredBox
                            {
                                Color = new Color(92, 76, 44),
                                Child = new OverflowBoxWidget
                                {
                                    Fit = OverflowBoxFit.DeferToChild,
                                    Child = Bar(new Color(255, 111, 97), 60, 60)
                                }
                            }
                        }),
                        new SizedBox { Width = 24 },

                        // SizedOverflowBox は自身の寸法(80x80)を指定し、
                        // 子は親から受け取った元の制約でレイアウトする。
                        // 幅120の子が茶色(自身の範囲)から左右へはみ出す。
                        Cell("SizedOverflowBox", new Center
                        {
                            Child = new ColoredBox
                            {
                                Color = new Color(92, 76, 44),
                                Child = new SizedOverflowBox
                                {
                                    Size = new Size(80, 80),
                                    Child = Bar(new Color(255, 209, 102), 120, 40)
                                }
                            }
                        })
                    }
                }
            }
        }
    };

    /// <summary>ラベル付きの枠を作り、その中で OverflowBox の効果を見せる。</summary>
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

    /// <summary>固定寸法を要求する帯。</summary>
    private static Widget Bar(Color color, double width, double height) => new ColoredBox
    {
        Color = color,
        Child = new SizedBox { Width = width, Height = height }
    };
}
