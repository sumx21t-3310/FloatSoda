using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.Painting;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using OpacityWidget = FloatSoda.Widgets.Paint.Opacity;

namespace FloatSoda.Samples.Opacity;

/// <summary>グループ単位の半透明合成と、0.0 でもレイアウト領域が残ることを確認するサンプルです。</summary>
public sealed record OpacityDemo : StatelessWidget
{
    /// <inheritdoc />
    public override Widget Build(IBuildContext context) => new SizedBox
    {
        Width = 640,
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
                        // 比較用の原色。重なった2つの四角をそのまま描く。
                        Cell("1.0(既定)", Pair()),
                        new SizedBox { Width = 24 },

                        // グループごと半透明にする。2つの四角がまとめて薄くなり、
                        // 重なり部分だけが二重に濃くなることはない。
                        Cell("0.5", new OpacityWidget
                        {
                            Value = 0.5,
                            Child = Pair()
                        }),
                        new SizedBox { Width = 24 },

                        // 0.0 は描画されないが、レイアウト領域は残る。
                        // 3つ並べた真ん中を 0.0 にすると、間隔が保たれたまま空く。
                        Cell("0.0 でも領域は残る", new Center
                        {
                            Child = new Row
                            {
                                MainAxisSize = MainAxisSize.Min,
                                Children =
                                {
                                    Marker(new Color(124, 205, 255)),
                                    new SizedBox { Width = 8 },
                                    new OpacityWidget
                                    {
                                        Value = 0.0,
                                        Child = Marker(new Color(255, 111, 97))
                                    },
                                    new SizedBox { Width = 8 },
                                    Marker(new Color(255, 209, 102))
                                }
                            }
                        })
                    }
                }
            }
        }
    };

    /// <summary>ラベル付きの枠を作り、その中で不透明度の効果を見せる。</summary>
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

    /// <summary>半透明合成の見え方を確認するための、重なった2つの四角。</summary>
    private static Widget Pair() => new Stack
    {
        Children =
        {
            new Positioned
            {
                Left = 20,
                Top = 20,
                Width = 70,
                Height = 70,
                Child = new ColoredBox { Color = new Color(124, 205, 255), Child = new SizedBox() }
            },
            new Positioned
            {
                Left = 60,
                Top = 60,
                Width = 70,
                Height = 70,
                Child = new ColoredBox { Color = new Color(255, 111, 97), Child = new SizedBox() }
            }
        }
    };

    private static Widget Marker(Color color) => new ColoredBox
    {
        Color = color,
        Child = new SizedBox { Width = 36, Height = 36 }
    };
}
