using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.Painting;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using SizedBoxWidget = FloatSoda.Widgets.Layout.SizedBox;

namespace FloatSoda.Samples.SizedBox;

/// <summary>寸法の固定、片方だけの指定、および余白としての使い方を確認するサンプルです。</summary>
public sealed record SizedBoxDemo : StatelessWidget
{
    /// <inheritdoc />
    public override Widget Build(IBuildContext context) => new SizedBoxWidget
    {
        Width = 940,
        Height = 460,
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
                        Label("Width と Height の両方を指定する"),
                        new SizedBoxWidget { Height = 12 },
                        new Row
                        {
                            CrossAxisAlignment = CrossAxisAlignment.Start,
                            Children =
                            {
                                Box(new Color(124, 205, 255), 200, 90),
                                // 子を持たない SizedBox は、そのぶんの空白として働く。
                                // Row / Column の要素間隔はこれで作る。
                                new SizedBoxWidget { Width = 40 },
                                Box(new Color(255, 111, 97), 120, 90),
                                new SizedBoxWidget { Width = 40 },
                                Box(new Color(255, 209, 102), 60, 90)
                            }
                        },
                        new SizedBoxWidget { Height = 40 },

                        Label("片方だけ指定すると、もう一方は親の制約に従う"),
                        new SizedBoxWidget { Height = 12 },

                        // Height だけを指定。Width は親から渡された制約のまま。
                        new SizedBoxWidget
                        {
                            Height = 70,
                            Child = new ColoredBox
                            {
                                Color = new Color(40, 47, 64),
                                Child = new Center
                                {
                                    Child = Label("Height = 70 のみ指定")
                                }
                            }
                        }
                    }
                }
            }
        }
    };

    private static Widget Label(string text) => new Text(text)
    {
        Style = new TextStyle { FontSize = 20, Color = new Color(169, 180, 204) }
    };

    private static Widget Box(Color color, double width, double height) => new ColoredBox
    {
        Color = color,
        Child = new SizedBoxWidget { Width = width, Height = height }
    };
}
