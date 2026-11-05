using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.Painting;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using DefaultTextStyleWidget = FloatSoda.Widgets.DefaultTextStyle;

namespace FloatSoda.Samples.DefaultTextStyle;

/// <summary>祖先から子孫の<see cref="Text"/>へ書式を引き継ぐ様子を確認するサンプルです。</summary>
public sealed record DefaultTextStyleDemo : StatelessWidget
{
    /// <inheritdoc />
    public override Widget Build(IBuildContext context) =>
        new SizedBox
        {
            Width = 900,
            Height = 360,
            Child = new ColoredBox
            {
                Color = new Color(16, 20, 31),
                Child = new Padding
                {
                    Spacing = EdgeInsets.All(40),
                    // サブツリー全体の既定書式を決める。配下の Text は Style を省略するとこの書式で描かれる。
                    Child = new DefaultTextStyleWidget
                    {
                        Style = new TextStyle { FontSize = 30, Color = new Color(244, 247, 255) },
                        Child = new Column
                        {
                            CrossAxisAlignment = CrossAxisAlignment.Start,
                            Children =
                            {
                                // Style を省略した Text。祖先の FontSize 30・白をそのまま使う。
                                new Text("Style を省略すると、祖先の書式を使う"),
                                new SizedBox { Height = 20 },
                                // 指定したプロパティだけが上書きされ、FontSize は祖先の 30 のまま残る。
                                new Text("Color だけを指定すると、色だけが変わる")
                                {
                                    Style = new TextStyle { Color = new Color(124, 205, 255) },
                                },
                                new SizedBox { Height = 20 },
                                new Text("FontWeight 700 を足すと、太字だけが加わる")
                                {
                                    Style = new TextStyle { FontWeight = 700 },
                                },
                                new SizedBox { Height = 20 },
                                // Inherit = false の書式は祖先と混ぜずに使う。
                                // 指定しなかった Color は祖先の白ではなく、フレームワークの既定の黒になる。
                                new ColoredBox
                                {
                                    Color = new Color(226, 232, 240),
                                    Child = new Padding
                                    {
                                        Spacing = EdgeInsets.All(8),
                                        Child = new Text("Inherit = false で、祖先の書式を使わない")
                                        {
                                            Style = new TextStyle
                                            {
                                                Inherit = false,
                                                FontSize = 24,
                                            },
                                        },
                                    },
                                },
                                new SizedBox { Height = 20 },
                                // 入れ子にすると、内側の DefaultTextStyle が外側を置き換える。
                                new DefaultTextStyleWidget
                                {
                                    Style = new TextStyle
                                    {
                                        FontSize = 22,
                                        Color = new Color(255, 111, 97),
                                        IsItalic = true,
                                    },
                                    Child = new Text("内側の DefaultTextStyle が外側に代わる"),
                                },
                            },
                        },
                    },
                },
            },
        };
}
