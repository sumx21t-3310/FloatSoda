using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.Painting;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using ColoredBoxWidget = FloatSoda.Widgets.Paint.ColoredBox;

namespace FloatSoda.Samples.ColoredBox;

/// <summary>単色の塗りつぶしと、子の有無によるサイズの決まり方を確認するサンプルです。</summary>
public sealed record ColoredBoxDemo : StatelessWidget
{
    /// <inheritdoc />
    public override Widget Build(IBuildContext context) => new SizedBox
    {
        Width = 900,
        Height = 420,
        Child = new ColoredBoxWidget
        {
            // 子を持つ ColoredBox は子のサイズに従う。ここでは外側の SizedBox が
            // 900 x 420 を与えるので、その全面が塗られる。
            Color = new Color(16, 20, 31),
            Child = new Padding
            {
                Spacing = EdgeInsets.All(40),
                Child = new Row
                {
                    CrossAxisAlignment = CrossAxisAlignment.Start,
                    Children =
                    {
                        // 子のサイズがそのまま塗る範囲になる。
                        new ColoredBoxWidget
                        {
                            Color = new Color(124, 205, 255),
                            Child = new SizedBox { Width = 200, Height = 160 }
                        },
                        new SizedBox { Width = 32 },

                        // 入れ子にすると、内側が外側の上に描かれる。
                        // Padding を挟んだぶんだけ外側の色が縁として残る。
                        new ColoredBoxWidget
                        {
                            Color = new Color(40, 47, 64),
                            Child = new Padding
                            {
                                Spacing = EdgeInsets.All(28),
                                Child = new ColoredBoxWidget
                                {
                                    Color = new Color(255, 111, 97),
                                    Child = new SizedBox { Width = 144, Height = 104 }
                                }
                            }
                        }
                    }
                }
            }
        }
    };
}
