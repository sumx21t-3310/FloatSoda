using System.Numerics;
using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.Painting;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using TransformWidget = FloatSoda.Widgets.Paint.Transform;

namespace FloatSoda.Samples.Transform;

/// <summary>回転・縮小・平行移動と、変換原点の違い、レイアウトが動かないことを確認するサンプルです。</summary>
public sealed record TransformDemo : StatelessWidget
{
    /// <inheritdoc />
    public override Widget Build(IBuildContext context) => new SizedBox
    {
        Width = 880,
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
                        // 原点を指定しない回転。既定の原点は子の左上で、
                        // 四角は左上の角を軸に振れる。
                        Cell("回転(原点は左上)", new Center
                        {
                            Child = new TransformWidget
                            {
                                Matrix = Matrix3x2.CreateRotation((float)(15 * Math.PI / 180)),
                                Child = Marker(new Color(124, 205, 255))
                            }
                        }),
                        new SizedBox { Width = 24 },

                        // Alignment.Center で原点を中心に移すと、その場で回る。
                        Cell("回転(中心が原点)", new Center
                        {
                            Child = new TransformWidget
                            {
                                Matrix = Matrix3x2.CreateRotation((float)(15 * Math.PI / 180)),
                                Alignment = Alignment.Center,
                                Child = Marker(new Color(255, 111, 97))
                            }
                        }),
                        new SizedBox { Width = 24 },

                        Cell("縮小 0.6", new Center
                        {
                            Child = new TransformWidget
                            {
                                Matrix = Matrix3x2.CreateScale(0.6f),
                                Alignment = Alignment.Center,
                                Child = Marker(new Color(255, 209, 102))
                            }
                        }),
                        new SizedBox { Width = 24 },

                        // 平行移動。描画だけが動き、レイアウト領域は元の位置のまま。
                        // 右へ 40 動かすと枠の端を越えて描かれる。
                        Cell("平行移動(描画のみ)", new Center
                        {
                            Child = new TransformWidget
                            {
                                Matrix = Matrix3x2.CreateTranslation(40, 28),
                                Child = Marker(new Color(124, 205, 255))
                            }
                        })
                    }
                }
            }
        }
    };

    /// <summary>ラベル付きの枠を作り、その中で変換の効果を見せる。</summary>
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

    private static Widget Marker(Color color) => new ColoredBox
    {
        Color = color,
        Child = new SizedBox { Width = 90, Height = 90 }
    };
}
