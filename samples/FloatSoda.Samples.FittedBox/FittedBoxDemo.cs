using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.Painting;
using FloatSoda.Rendering.Layers;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using FittedBoxWidget = FloatSoda.Widgets.Layout.FittedBox;

namespace FloatSoda.Samples.FittedBox;

/// <summary>BoxFit ごとの拡大縮小と、はみ出し時のクリップの有無を確認するサンプルです。</summary>
public sealed record FittedBoxDemo : StatelessWidget
{
    /// <inheritdoc />
    public override Widget Build(IBuildContext context) => new SizedBox
    {
        Width = 960,
        Height = 240,
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
                        // 全体が収まる範囲で比率を維持して最大化する(既定)。
                        Cell("Contain(既定)", new FittedBoxWidget
                        {
                            Child = Flag()
                        }),
                        new SizedBox { Width = 24 },

                        // 比率を無視して領域全体へ引き伸ばす。目印の正方形が歪む。
                        Cell("Fill", new FittedBoxWidget
                        {
                            Fit = BoxFit.Fill,
                            Child = Flag()
                        }),
                        new SizedBox { Width = 24 },

                        // 拡大縮小しない。子は自然な大きさのまま中央に置かれる。
                        Cell("None", new FittedBoxWidget
                        {
                            Fit = BoxFit.None,
                            Child = Flag()
                        }),
                        new SizedBox { Width = 24 },

                        // 領域全体を覆う範囲で比率を維持する。はみ出しをクリップで切り取る。
                        Cell("Cover + HardEdge", new FittedBoxWidget
                        {
                            Fit = BoxFit.Cover,
                            ClipBehavior = Clip.HardEdge,
                            Child = Flag()
                        }),
                        new SizedBox { Width = 24 },

                        // ClipBehavior の既定は Clip.None。Cover のはみ出しがそのまま見える。
                        Cell("Cover(クリップなし)", new FittedBoxWidget
                        {
                            Fit = BoxFit.Cover,
                            Child = Flag()
                        })
                    }
                }
            }
        }
    };

    /// <summary>ラベル付きの枠を作り、その中で FittedBox の効果を見せる。</summary>
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
                    Height = 100,
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

    /// <summary>拡大縮小が分かるように、右下へ目印の正方形を置いた 120x64 の旗。</summary>
    private static Widget Flag() => new SizedBox
    {
        Width = 120,
        Height = 64,
        Child = new Stack
        {
            // Expand で下地の ColoredBox を旗全体に広げる。
            Fit = StackFit.Expand,
            Children =
            {
                new ColoredBox
                {
                    Color = new Color(124, 205, 255),
                    Child = new SizedBox()
                },
                new Positioned
                {
                    Right = 8,
                    Bottom = 8,
                    Width = 32,
                    Height = 32,
                    Child = new ColoredBox
                    {
                        Color = new Color(255, 111, 97),
                        Child = new SizedBox()
                    }
                }
            }
        }
    };
}
