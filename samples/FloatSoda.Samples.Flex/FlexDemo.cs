using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.Painting;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using FlexWidget = FloatSoda.Widgets.Layout.Flex;

namespace FloatSoda.Samples.Flex;

/// <summary>主軸・交差軸の揃え方と、Row / Column との対応を確認するサンプルです。</summary>
public sealed record FlexDemo : StatelessWidget
{
    /// <inheritdoc />
    public override Widget Build(IBuildContext context) => new SizedBox
    {
        Width = 980,
        Height = 620,
        Child = new ColoredBox
        {
            Color = new Color(16, 20, 31),
            Child = new Padding
            {
                Spacing = EdgeInsets.All(32),
                Child = new Column
                {
                    CrossAxisAlignment = CrossAxisAlignment.Start,
                    Children =
                    {
                        Label("MainAxisAlignment — 主軸方向の余りの配り方"),
                        new SizedBox { Height = 10 },
                        Track(MainAxisAlignment.Start, "Start"),
                        new SizedBox { Height = 8 },
                        Track(MainAxisAlignment.Center, "Center"),
                        new SizedBox { Height = 8 },
                        Track(MainAxisAlignment.SpaceBetween, "SpaceBetween"),
                        new SizedBox { Height = 28 },

                        Label("CrossAxisAlignment — 交差軸方向の揃え方"),
                        new SizedBox { Height = 10 },
                        CrossTrack(CrossAxisAlignment.Start, "Start"),
                        new SizedBox { Height = 8 },
                        CrossTrack(CrossAxisAlignment.Center, "Center"),
                        new SizedBox { Height = 28 },

                        // Row / Column は Flex に Direction を固定した薄いラッパー。
                        Label("Column は Flex の Direction = Vertical と同じ"),
                        new SizedBox { Height = 10 },
                        new ColoredBox
                        {
                            Color = new Color(40, 47, 64),
                            Child = new SizedBox
                            {
                                Height = 96,
                                Child = new FlexWidget
                                {
                                    Direction = Axis.Vertical,
                                    MainAxisAlignment = MainAxisAlignment.SpaceBetween,
                                    Children =
                                    {
                                        Bar(new Color(124, 205, 255), 160, 24),
                                        Bar(new Color(255, 111, 97), 220, 24)
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    };

    /// <summary>主軸の揃え方を1本の帯として見せる。</summary>
    private static Widget Track(MainAxisAlignment alignment, string label) => new Row
    {
        CrossAxisAlignment = CrossAxisAlignment.Center,
        Children =
        {
            new SizedBox { Width = 170, Child = Label(label) },
            new ColoredBox
            {
                Color = new Color(40, 47, 64),
                Child = new SizedBox
                {
                    Width = 620,
                    Height = 56,
                    Child = new FlexWidget
                    {
                        Direction = Axis.Horizontal,
                        MainAxisAlignment = alignment,
                        Children =
                        {
                            Bar(new Color(124, 205, 255), 90, 40),
                            Bar(new Color(255, 111, 97), 90, 40),
                            Bar(new Color(255, 209, 102), 90, 40)
                        }
                    }
                }
            }
        }
    };

    /// <summary>交差軸の揃え方を、高さの異なる子で見せる。</summary>
    private static Widget CrossTrack(CrossAxisAlignment alignment, string label) => new Row
    {
        CrossAxisAlignment = CrossAxisAlignment.Center,
        Children =
        {
            new SizedBox { Width = 170, Child = Label(label) },
            new ColoredBox
            {
                Color = new Color(40, 47, 64),
                Child = new SizedBox
                {
                    Width = 620,
                    Height = 72,
                    Child = new FlexWidget
                    {
                        Direction = Axis.Horizontal,
                        CrossAxisAlignment = alignment,
                        Children =
                        {
                            Bar(new Color(124, 205, 255), 90, 24),
                            Bar(new Color(255, 111, 97), 90, 48),
                            Bar(new Color(255, 209, 102), 90, 64)
                        }
                    }
                }
            }
        }
    };

    private static Widget Label(string text) => new Text(text)
    {
        Style = new TextStyle { FontSize = 18, Color = new Color(169, 180, 204) }
    };

    private static Widget Bar(Color color, double width, double height) => new ColoredBox
    {
        Color = color,
        Child = new SizedBox { Width = width, Height = height }
    };
}
