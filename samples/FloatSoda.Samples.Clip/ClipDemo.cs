using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.Painting;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using ClipMode = FloatSoda.Rendering.Layers.Clip;

namespace FloatSoda.Samples.Clip;

/// <summary>矩形・角丸・楕円による切り抜きと、ClipBehavior の違いを確認するサンプルです。</summary>
public sealed record ClipDemo : StatelessWidget
{
    /// <inheritdoc />
    public override Widget Build(IBuildContext context) => new SizedBox
    {
        Width = 960,
        Height = 420,
        Child = new ColoredBox
        {
            Color = new Color(16, 20, 31),
            Child = new Padding
            {
                Spacing = EdgeInsets.All(32),
                Child = new Row
                {
                    CrossAxisAlignment = CrossAxisAlignment.Start,
                    Children =
                    {
                        // 切り抜き無し。はみ出した子がそのまま見える。
                        Cell("切り抜き無し", Overflowing()),
                        new SizedBox { Width = 24 },

                        // ClipRect は自身の領域の矩形で切り抜く。
                        Cell("ClipRect", new ClipRect { Child = Overflowing() }),
                        new SizedBox { Width = 24 },

                        // ClipRoundRect は BorderRadius で角を丸める。
                        // BorderRadius は init プロパティではなくフィールドとして宣言されている。
                        Cell("ClipRoundRect", new ClipRoundRect
                        {
                            BorderRadius = BorderRadius.Circular(28),
                            Child = Overflowing()
                        }),
                        new SizedBox { Width = 24 },

                        // ClipOval は領域に内接する楕円で切り抜く。
                        Cell("ClipOval", new ClipOval { Child = Overflowing() }),
                        new SizedBox { Width = 24 },

                        // ClipBehavior.HardEdge はアンチエイリアスを行わない。
                        // 既定は Antialias で、曲線の縁が滑らかになる。
                        Cell("HardEdge", new ClipOval
                        {
                            ClipBehavior = ClipMode.HardEdge,
                            Child = Overflowing()
                        })
                    }
                }
            }
        }
    };

    /// <summary>切り抜き対象。領域より大きい子を置いてはみ出させる。</summary>
    private static Widget Overflowing() => new SizedBox
    {
        Width = 150,
        Height = 150,
        Child = new Center
        {
            Child = new ColoredBox
            {
                Color = new Color(124, 205, 255),
                Child = new SizedBox { Width = 190, Height = 110 }
            }
        }
    };

    private static Widget Cell(string label, Widget child) => new Column
    {
        MainAxisSize = MainAxisSize.Min,
        Children =
        {
            new ColoredBox
            {
                Color = new Color(40, 47, 64),
                Child = new SizedBox { Width = 150, Height = 150, Child = child }
            },
            new SizedBox { Height = 10 },
            new Text(label)
            {
                Style = new TextStyle { FontSize = 17, Color = new Color(169, 180, 204) }
            }
        }
    };
}
