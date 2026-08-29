using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.Painting;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using StackWidget = FloatSoda.Widgets.Layout.Stack;

namespace FloatSoda.Samples.Stack;

/// <summary>重ね順、Alignment、Positioned による絶対配置、はみ出しの扱いを確認するサンプルです。</summary>
public sealed record StackDemo : StatelessWidget
{
    /// <inheritdoc />
    public override Widget Build(IBuildContext context) => new SizedBox
    {
        Width = 940,
        Height = 300,
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
                        // 子はリストの順に下から積まれる。後の子ほど手前に描かれる。
                        Cell("重ね順", new StackWidget
                        {
                            Alignment = Alignment.Center,
                            Children =
                            {
                                Marker(new Color(124, 205, 255), 110),
                                Marker(new Color(255, 111, 97), 80),
                                Marker(new Color(255, 209, 102), 50)
                            }
                        }),
                        new SizedBox { Width = 24 },

                        // Positioned を使わない子は、Alignment の位置へまとめて配置される。
                        Cell("Alignment.BottomRight", new StackWidget
                        {
                            Alignment = Alignment.BottomRight,
                            Children =
                            {
                                Marker(new Color(124, 205, 255), 90),
                                Marker(new Color(255, 111, 97), 50)
                            }
                        }),
                        new SizedBox { Width = 24 },

                        // Positioned で辺からの距離を指定する。
                        // Left/Top は左上から、Right/Bottom は右下からの距離。
                        Cell("Positioned", new StackWidget
                        {
                            Children =
                            {
                                new Positioned
                                {
                                    Left = 10,
                                    Top = 10,
                                    Child = Marker(new Color(124, 205, 255), 48)
                                },
                                new Positioned
                                {
                                    Right = 10,
                                    Bottom = 10,
                                    Child = Marker(new Color(255, 111, 97), 48)
                                }
                            }
                        }),
                        new SizedBox { Width = 24 },

                        // 同じ軸の両端(Left と Right)を指定すると、子はその間へ引き伸ばされる。
                        Cell("Left+Right で引き伸ばし", new StackWidget
                        {
                            Children =
                            {
                                new Positioned
                                {
                                    Left = 10,
                                    Right = 10,
                                    Top = 59,
                                    Height = 32,
                                    Child = Fill(new Color(255, 209, 102))
                                }
                            }
                        }),
                        new SizedBox { Width = 24 },

                        // Stack の範囲からはみ出た子はクリップされず、そのまま描かれる。
                        // Flutter の既定(Clip.hardEdge)とは異なる。README の「Flutterとの違い」を参照。
                        Cell("はみ出しは見える", new StackWidget
                        {
                            Children =
                            {
                                new Positioned
                                {
                                    Left = 118,
                                    Top = 47,
                                    Width = 56,
                                    Height = 56,
                                    Child = Fill(new Color(255, 111, 97))
                                }
                            }
                        })
                    }
                }
            }
        }
    };

    /// <summary>ラベル付きの枠を作り、その中で Stack の効果を見せる。</summary>
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

    private static Widget Marker(Color color, double size) => new ColoredBox
    {
        Color = color,
        Child = new SizedBox { Width = size, Height = size }
    };

    /// <summary>与えられた領域いっぱいに広がる塗り。引き伸ばしの範囲を可視化する。</summary>
    private static Widget Fill(Color color) => new ColoredBox
    {
        Color = color,
        Child = new SizedBox()
    };
}
