using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.Painting;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using ExpandedWidget = FloatSoda.Widgets.Layout.Expanded;

namespace FloatSoda.Samples.Expanded;

/// <summary>余剰領域の比率分配と、Expanded / Flexible / Spacer の違いを確認するサンプルです。</summary>
public sealed record ExpandedDemo : StatelessWidget
{
    /// <inheritdoc />
    public override Widget Build(IBuildContext context) => new SizedBox
    {
        Width = 880,
        Height = 560,
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
                        Label("Expanded — 余剰領域を Flex の比率で分ける"),
                        new SizedBox { Height = 10 },
                        // Flex 比 1:2:1。620 の幅が 155 / 310 / 155 に分かれる。
                        Track("1 : 2 : 1", new Row
                        {
                            CrossAxisAlignment = CrossAxisAlignment.Center,
                            Children =
                            {
                                new ExpandedWidget { Child = FlexBar(new Color(124, 205, 255)) },
                                new ExpandedWidget { Flex = 2, Child = FlexBar(new Color(255, 111, 97)) },
                                new ExpandedWidget { Child = FlexBar(new Color(255, 209, 102)) }
                            }
                        }),
                        new SizedBox { Height = 24 },

                        Label("固定幅の子と混在 — 固定分を引いた残りが余剰領域"),
                        new SizedBox { Height = 10 },
                        Track("固定 220 + Expanded", new Row
                        {
                            CrossAxisAlignment = CrossAxisAlignment.Center,
                            Children =
                            {
                                Bar(new Color(124, 205, 255), 220, 40),
                                new ExpandedWidget { Child = FlexBar(new Color(255, 111, 97)) }
                            }
                        }),
                        new SizedBox { Height = 24 },

                        // どちらも幅90を要求する SizedBox を子に持つ。
                        // Expanded(Tight)は割当量いっぱいへ引き伸ばし、Flexible(Loose)は要求を尊重する。
                        Label("Expanded と Flexible — 同じ子(幅90)でも結果が違う"),
                        new SizedBox { Height = 10 },
                        Track("Expanded", new Row
                        {
                            CrossAxisAlignment = CrossAxisAlignment.Center,
                            Children =
                            {
                                new ExpandedWidget
                                {
                                    Child = new ColoredBox
                                    {
                                        Color = new Color(124, 205, 255),
                                        Child = new SizedBox { Width = 90, Height = 40 }
                                    }
                                }
                            }
                        }),
                        new SizedBox { Height = 8 },
                        Track("Flexible", new Row
                        {
                            CrossAxisAlignment = CrossAxisAlignment.Center,
                            Children =
                            {
                                new Flexible
                                {
                                    Child = new ColoredBox
                                    {
                                        Color = new Color(255, 111, 97),
                                        Child = new SizedBox { Width = 90, Height = 40 }
                                    }
                                }
                            }
                        }),
                        new SizedBox { Height = 24 },

                        Label("Spacer — 比率指定できる空白"),
                        new SizedBox { Height = 10 },
                        Track("Spacer 1 : 2", new Row
                        {
                            CrossAxisAlignment = CrossAxisAlignment.Center,
                            Children =
                            {
                                Bar(new Color(124, 205, 255), 90, 40),
                                new Spacer(),
                                Bar(new Color(255, 111, 97), 90, 40),
                                new Spacer { Flex = 2 },
                                Bar(new Color(255, 209, 102), 90, 40)
                            }
                        })
                    }
                }
            }
        }
    };

    /// <summary>ラベルと固定幅の帯を組にして、余剰領域の配り方を1本の帯として見せる。</summary>
    private static Widget Track(string label, Widget content) => new Row
    {
        CrossAxisAlignment = CrossAxisAlignment.Center,
        Children =
        {
            new SizedBox { Width = 190, Child = Label(label) },
            new ColoredBox
            {
                Color = new Color(40, 47, 64),
                Child = new SizedBox
                {
                    Width = 620,
                    Height = 56,
                    Child = content
                }
            }
        }
    };

    private static Widget Label(string text) => new Text(text)
    {
        Style = new TextStyle { FontSize = 18, Color = new Color(169, 180, 204) }
    };

    /// <summary>固定寸法の帯。flex を持たない子として余剰領域の計算から除かれる。</summary>
    private static Widget Bar(Color color, double width, double height) => new ColoredBox
    {
        Color = color,
        Child = new SizedBox { Width = width, Height = height }
    };

    /// <summary>幅を固定しない帯。Expanded が割り当てた幅いっぱいに広がる。</summary>
    private static Widget FlexBar(Color color) => new ColoredBox
    {
        Color = color,
        Child = new SizedBox { Height = 40 }
    };
}
