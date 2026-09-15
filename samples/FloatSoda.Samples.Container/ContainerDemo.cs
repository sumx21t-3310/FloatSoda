using System.Numerics;
using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.Painting;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using ContainerWidget = FloatSoda.Widgets.Layout.Container;

namespace FloatSoda.Samples.Container;

/// <summary>Container が合成する装飾・余白・配置・寸法・変換を1プロパティずつ確認するサンプルです。</summary>
public sealed record ContainerDemo : StatelessWidget
{
    /// <inheritdoc />
    public override Widget Build(IBuildContext context) => new SizedBox
    {
        Width = 920,
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
                        // Color と Width/Height だけの最小形。96x96 の単色の四角になる。
                        Cell("Color + 寸法", new Center
                        {
                            Child = new ContainerWidget
                            {
                                Width = 96,
                                Height = 96,
                                Color = new Color(124, 205, 255)
                            }
                        }),
                        new SizedBox { Width = 24 },

                        // BoxDecoration で角丸とボーダーを付ける。
                        // 背景色は Color ではなく BoxDecoration.Color へ指定する。
                        Cell("Decoration", new Center
                        {
                            Child = new ContainerWidget
                            {
                                Width = 110,
                                Height = 110,
                                Decoration = new BoxDecoration
                                {
                                    Color = new Color(255, 111, 97),
                                    BorderRadius = BorderRadius.Circular(16),
                                    Border = Border.All(new BorderSide
                                    {
                                        Color = new Color(255, 209, 102),
                                        Width = 4
                                    })
                                }
                            }
                        }),
                        new SizedBox { Width = 24 },

                        // Padding は装飾の内側に入る。子の水色は各辺16だけ内側へ寄り、
                        // 装飾より小さくなるため角丸にも重ならない。
                        Cell("Padding", new Center
                        {
                            Child = new ContainerWidget
                            {
                                Width = 110,
                                Height = 110,
                                Padding = EdgeInsets.All(16),
                                Decoration = new BoxDecoration
                                {
                                    Color = new Color(255, 209, 102),
                                    BorderRadius = BorderRadius.Circular(16)
                                },
                                Child = Fill(new Color(124, 205, 255))
                            }
                        }),
                        new SizedBox { Width = 24 },

                        // Alignment で子を領域内へ配置する。Container 自身は枠いっぱいに広がり、
                        // 40x40 の子が右下へ寄る。
                        Cell("Alignment", new ContainerWidget
                        {
                            Alignment = Alignment.BottomRight,
                            Child = new SizedBox
                            {
                                Width = 40,
                                Height = 40,
                                Child = Fill(new Color(255, 111, 97))
                            }
                        }),
                        new SizedBox { Width = 24 },

                        // Transform はレイアウト後に適用される。80x80 の四角が中心を軸に22.5度回転する。
                        Cell("Transform", new Center
                        {
                            Child = new ContainerWidget
                            {
                                Width = 80,
                                Height = 80,
                                Color = new Color(124, 205, 255),
                                Transform = Matrix3x2.CreateRotation(MathF.PI / 8f),
                                TransformAlignment = Alignment.Center
                            }
                        })
                    }
                }
            }
        }
    };

    /// <summary>ラベル付きの枠を作り、その中で Container の効果を見せる。</summary>
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

    /// <summary>与えられた領域いっぱいに広がる塗り。余白の内側を可視化する。</summary>
    private static Widget Fill(Color color) => new ColoredBox
    {
        Color = color,
        Child = new SizedBox()
    };
}
