using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.Painting;
using FloatSoda.RenderObjects;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using TextWidget = FloatSoda.Widgets.Text;

namespace FloatSoda.Samples.Text;

/// <summary>文字列の表示と書式指定、および書式付きテキストの表示を確認するサンプルです。</summary>
public sealed record TextDemo : StatelessWidget
{
    /// <inheritdoc />
    public override Widget Build(IBuildContext context) => new SizedBox
    {
        Width = 900,
        Height = 520,
        Child = new ColoredBox
        {
            Color = new Color(16, 20, 31),
            Child = new Padding
            {
                Spacing = EdgeInsets.All(40),
                Child = new Column
                {
                    CrossAxisAlignment = CrossAxisAlignment.Start,
                    Children =
                    {
                        // Style を省略すると段落の既定書式で描画される。
                        new TextWidget("Style を指定しない既定の表示"),
                        new SizedBox { Height = 28 },

                        // FontSize と Color だけを変える。
                        new TextWidget("FontSize 36 / 明るい前景色")
                        {
                            Style = new TextStyle
                            {
                                FontSize = 36,
                                Color = new Color(244, 247, 255)
                            }
                        },
                        new SizedBox { Height = 16 },

                        // FontWeight は int で指定する。
                        new TextWidget("FontWeight 700 の太字")
                        {
                            Style = new TextStyle
                            {
                                FontSize = 32,
                                Color = new Color(124, 205, 255),
                                FontWeight = 700
                            }
                        },
                        new SizedBox { Height = 16 },

                        new TextWidget("IsItalic による斜体")
                        {
                            Style = new TextStyle
                            {
                                FontSize = 32,
                                Color = new Color(255, 111, 97),
                                IsItalic = true
                            }
                        },
                        new SizedBox { Height = 36 },

                        // RichText は TextSpan を直接受け取る。Text はこの上の薄いラッパー。
                        new RichText
                        {
                            Text = new TextSpan("RichText と TextSpan による表示")
                            {
                                Style = new TextStyle
                                {
                                    FontSize = 28,
                                    Color = new Color(169, 180, 204)
                                }
                            }
                        }
                    }
                }
            }
        }
    };
}
