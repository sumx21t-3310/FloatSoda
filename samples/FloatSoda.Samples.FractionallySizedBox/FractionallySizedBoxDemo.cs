using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.Painting;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using FractionallySizedBoxWidget = FloatSoda.Widgets.Layout.FractionallySizedBox;

namespace FloatSoda.Samples.FractionallySizedBox;

/// <summary>親に対する割合での寸法指定と、配置・1超の係数でのはみ出しを確認するサンプルです。</summary>
public sealed record FractionallySizedBoxDemo : StatelessWidget
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
                        // 幅だけ親の 0.5 倍。指定しなかった高さは親の制約がそのまま渡る。
                        Cell("WidthFactor = 0.5", new FractionallySizedBoxWidget
                        {
                            WidthFactor = 0.5,
                            Child = Fill(new Color(124, 205, 255))
                        }),
                        new SizedBox { Width = 24 },

                        // 両軸とも 0.5 倍。子は親の面積の 1/4 になり、既定では中央に置かれる。
                        Cell("0.5 x 0.5", new FractionallySizedBoxWidget
                        {
                            WidthFactor = 0.5,
                            HeightFactor = 0.5,
                            Child = Fill(new Color(255, 111, 97))
                        }),
                        new SizedBox { Width = 24 },

                        Cell("Alignment.TopLeft", new FractionallySizedBoxWidget
                        {
                            WidthFactor = 0.5,
                            HeightFactor = 0.5,
                            Alignment = Alignment.TopLeft,
                            Child = Fill(new Color(255, 209, 102))
                        }),
                        new SizedBox { Width = 24 },

                        // 係数は1を超えてもよい。子は親より大きくなり、
                        // はみ出しはクリップされずそのまま描かれる。
                        Cell("WidthFactor = 1.4", new FractionallySizedBoxWidget
                        {
                            WidthFactor = 1.4,
                            HeightFactor = 0.3,
                            Child = Fill(new Color(124, 205, 255))
                        })
                    }
                }
            }
        }
    };

    /// <summary>ラベル付きの枠を作り、その中で割合指定の効果を見せる。</summary>
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

    /// <summary>与えられた領域いっぱいに広がる塗り。割合で決まった範囲を可視化する。</summary>
    private static Widget Fill(Color color) => new ColoredBox
    {
        Color = color,
        Child = new SizedBox()
    };
}
