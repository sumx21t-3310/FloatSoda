using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.Painting;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using PaddingWidget = FloatSoda.Widgets.Layout.Padding;

namespace FloatSoda.Samples.Padding;

/// <summary>EdgeInsets の作り方ごとの余白の付き方と、子に合わせた収縮を確認するサンプルです。</summary>
public sealed record PaddingDemo : StatelessWidget
{
    /// <inheritdoc />
    public override Widget Build(IBuildContext context) => new SizedBox
    {
        Width = 760,
        Height = 280,
        Child = new ColoredBox
        {
            Color = new Color(16, 20, 31),
            Child = new PaddingWidget
            {
                Spacing = EdgeInsets.All(32),
                Child = new Row
                {
                    Children =
                    {
                        // 四辺へ同じ余白を付ける。塗りが118x118へ縮み、余白が額縁として見える。
                        Cell("All(16)", new PaddingWidget
                        {
                            Spacing = EdgeInsets.All(16),
                            Child = Fill(new Color(124, 205, 255))
                        }),
                        new SizedBox { Width = 24 },

                        // 縦横で別々の余白を付ける。左右のほうが広い額縁になる。
                        Cell("Symmetric(12, 36)", new PaddingWidget
                        {
                            Spacing = EdgeInsets.Symmetric(vertical: 12, horizontal: 36),
                            Child = Fill(new Color(255, 111, 97))
                        }),
                        new SizedBox { Width = 24 },

                        // 一部の辺だけ余白を付ける。指定しなかった辺は0のまま。
                        Cell("Left だけ 48", new PaddingWidget
                        {
                            Spacing = new EdgeInsets(Left: 48),
                            Child = Fill(new Color(255, 209, 102))
                        }),
                        new SizedBox { Width = 24 },

                        // Padding 自身は「子の寸法 + 余白」の大きさになる。
                        // Center を挟んで緩い制約を作ると、枠いっぱいではなく
                        // 48x48 の子 + 各辺16 = 80x80 まで縮む。茶色が Padding 自身の範囲。
                        Cell("子に合わせて縮む", new Center
                        {
                            Child = new ColoredBox
                            {
                                Color = new Color(92, 76, 44),
                                Child = new PaddingWidget
                                {
                                    Spacing = EdgeInsets.All(16),
                                    Child = new ColoredBox
                                    {
                                        Color = new Color(124, 205, 255),
                                        Child = new SizedBox { Width = 48, Height = 48 }
                                    }
                                }
                            }
                        })
                    }
                }
            }
        }
    };

    /// <summary>ラベル付きの枠を作り、その中で Padding の効果を見せる。</summary>
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
