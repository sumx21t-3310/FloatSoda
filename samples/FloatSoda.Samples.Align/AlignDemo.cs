using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.Painting;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using AlignWidget = FloatSoda.Widgets.Layout.Align;

namespace FloatSoda.Samples.Align;

/// <summary>子の配置位置と、WidthFactor / HeightFactor による収縮を確認するサンプルです。</summary>
public sealed record AlignDemo : StatelessWidget
{
    /// <inheritdoc />
    public override Widget Build(IBuildContext context) => new SizedBox
    {
        Width = 940,
        Height = 420,
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
                        // Alignment のプリセットで、与えられた領域内の配置位置を決める。
                        Cell("TopLeft", new AlignWidget
                        {
                            Alignment = Alignment.TopLeft,
                            Child = Marker(new Color(124, 205, 255))
                        }),
                        new SizedBox { Width = 24 },

                        Cell("Center", new AlignWidget
                        {
                            Alignment = Alignment.Center,
                            Child = Marker(new Color(124, 205, 255))
                        }),
                        new SizedBox { Width = 24 },

                        Cell("BottomRight", new AlignWidget
                        {
                            Alignment = Alignment.BottomRight,
                            Child = Marker(new Color(124, 205, 255))
                        }),
                        new SizedBox { Width = 24 },

                        // Center は Align.Center への薄いラッパー。上の Center と同じ結果になる。
                        Cell("Center ウィジェット", new Center
                        {
                            Child = Marker(new Color(255, 111, 97))
                        }),
                        new SizedBox { Width = 24 },

                        // WidthFactor / HeightFactor を指定すると、Align 自身が
                        // 「子の寸法 x 係数」まで収縮する。領域いっぱいには広がらない。
                        Cell("係数 2.0 で収縮", new AlignWidget
                        {
                            Alignment = Alignment.Center,
                            WidthFactor = 2.0,
                            HeightFactor = 2.0,
                            Child = Marker(new Color(255, 209, 102))
                        })
                    }
                }
            }
        }
    };

    /// <summary>ラベル付きの枠を作り、その中で Align の効果を見せる。</summary>
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
        Child = new SizedBox { Width = 48, Height = 48 }
    };
}
