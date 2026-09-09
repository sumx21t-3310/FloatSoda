using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.Painting;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using LimitedBoxWidget = FloatSoda.Widgets.Layout.LimitedBox;

namespace FloatSoda.Samples.LimitedBox;

/// <summary>bounded な場所では何もせず、unbounded な場所でだけ上限になることを確認するサンプルです。</summary>
public sealed record LimitedBoxDemo : StatelessWidget
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
                        // 親が上限を与えている(bounded)軸では、MaxWidth / MaxHeight は無視され、
                        // 親の制約がそのまま子へ渡る。ここでは枠の tight 制約が勝ち、塗りが枠全体に広がる。
                        Cell("bounded では何もしない", new LimitedBoxWidget
                        {
                            MaxWidth = 80,
                            MaxHeight = 80,
                            Child = Fill(new Color(124, 205, 255))
                        }),
                        new SizedBox { Width = 24 },

                        // UnconstrainedBox で制約を取り除いた(unbounded)場所では、
                        // MaxWidth が上限として効く。幅500を要求する子が100で止まる。
                        Cell("unbounded で上限になる", new UnconstrainedBox
                        {
                            Child = new LimitedBoxWidget
                            {
                                MaxWidth = 100,
                                MaxHeight = 100,
                                Child = Bar(new Color(255, 111, 97), 500, 40)
                            }
                        }),
                        new SizedBox { Width = 24 },

                        // 比較: LimitedBox なしだと、子は要求どおりの幅になり枠からはみ出す。
                        Cell("LimitedBox なしの比較", new UnconstrainedBox
                        {
                            Child = Bar(new Color(255, 209, 102), 220, 40)
                        })
                    }
                }
            }
        }
    };

    /// <summary>ラベル付きの枠を作り、その中で LimitedBox の効果を見せる。</summary>
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

    /// <summary>与えられた領域いっぱいに広がる塗り。</summary>
    private static Widget Fill(Color color) => new ColoredBox
    {
        Color = color,
        Child = new SizedBox()
    };

    /// <summary>固定寸法を要求する帯。</summary>
    private static Widget Bar(Color color, double width, double height) => new ColoredBox
    {
        Color = color,
        Child = new SizedBox { Width = width, Height = height }
    };
}
