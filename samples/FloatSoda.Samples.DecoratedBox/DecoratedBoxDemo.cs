using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.Painting;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using DecoratedBoxWidget = FloatSoda.Widgets.Paint.DecoratedBox;

namespace FloatSoda.Samples.DecoratedBox;

/// <summary>背景色・角丸・ボーダーの組み合わせと、前面描画(Foreground)を確認するサンプルです。</summary>
public sealed record DecoratedBoxDemo : StatelessWidget
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
                        // 背景色と角丸。装飾は自身の寸法(ここでは子の 110x110)に描かれる。
                        Cell("背景色と角丸", new Center
                        {
                            Child = new DecoratedBoxWidget
                            {
                                Decoration = new BoxDecoration
                                {
                                    Color = new Color(124, 205, 255),
                                    BorderRadius = BorderRadius.Circular(24)
                                },
                                Child = new SizedBox { Width = 110, Height = 110 }
                            }
                        }),
                        new SizedBox { Width = 24 },

                        // 四辺へ同じボーダーを引く。
                        Cell("ボーダー", new Center
                        {
                            Child = new DecoratedBoxWidget
                            {
                                Decoration = new BoxDecoration
                                {
                                    Color = new Color(255, 111, 97),
                                    Border = Border.All(new BorderSide
                                    {
                                        Color = new Color(255, 209, 102),
                                        Width = 6
                                    })
                                },
                                Child = new SizedBox { Width = 110, Height = 110 }
                            }
                        }),
                        new SizedBox { Width = 24 },

                        // 角丸とボーダーの組み合わせ。ボーダーも角丸に沿う。
                        Cell("角丸ボーダー", new Center
                        {
                            Child = new DecoratedBoxWidget
                            {
                                Decoration = new BoxDecoration
                                {
                                    Color = new Color(40, 47, 64),
                                    BorderRadius = BorderRadius.Circular(24),
                                    Border = Border.All(new BorderSide
                                    {
                                        Color = new Color(124, 205, 255),
                                        Width = 4
                                    })
                                },
                                Child = new SizedBox { Width = 110, Height = 110 }
                            }
                        }),
                        new SizedBox { Width = 24 },

                        // Position = Foreground は子の前面へ描く。
                        // 半透明の黄をかぶせ、下の水色が透けることで前面描画を確認する。
                        Cell("Foreground", new Center
                        {
                            Child = new DecoratedBoxWidget
                            {
                                Position = DecorationPosition.Foreground,
                                Decoration = new BoxDecoration
                                {
                                    Color = new Color(255, 209, 102, 140)
                                },
                                Child = new ColoredBox
                                {
                                    Color = new Color(124, 205, 255),
                                    Child = new SizedBox { Width = 110, Height = 110 }
                                }
                            }
                        })
                    }
                }
            }
        }
    };

    /// <summary>ラベル付きの枠を作り、その中で装飾の効果を見せる。</summary>
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
}
