using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.Painting;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using VisibilityWidget = FloatSoda.Widgets.Layout.Visibility;

namespace FloatSoda.Samples.Visibility;

/// <summary>子を隠す・切り替える・90度ずつ回すウィジェットの表示を確認するサンプルです。</summary>
public sealed record VisibilityDemo : StatelessWidget
{
    /// <inheritdoc />
    public override Widget Build(IBuildContext context) =>
        new SizedBox
        {
            Width = 940,
            Height = 720,
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
                            Label("Visibility — Visible = false で子の代わりに Replacement を出す"),
                            new SizedBox { Height = 10 },
                            new Row
                            {
                                Children =
                                {
                                    // Replacement を省略すると大きさ0の SizedBox になり、青と黄が隙間なく並ぶ。
                                    Marker(new Color(124, 205, 255)),
                                    new VisibilityWidget
                                    {
                                        Visible = false,
                                        Child = Marker(new Color(255, 111, 97)),
                                    },
                                    Marker(new Color(255, 209, 102)),
                                    new SizedBox { Width = 48 },
                                    // Replacement を指定すると、隠した子の位置にそれを出す。
                                    new VisibilityWidget
                                    {
                                        Visible = false,
                                        Child = Marker(new Color(255, 111, 97)),
                                        Replacement = Marker(new Color(90, 98, 117)),
                                    },
                                    new SizedBox { Width = 48 },
                                    new VisibilityWidget
                                    {
                                        Visible = true,
                                        Child = Marker(new Color(255, 111, 97)),
                                    },
                                },
                            },
                            new SizedBox { Height = 28 },
                            Label("Offstage — IsOffstage = true の子は描かず、場所も取らない"),
                            new SizedBox { Height = 10 },
                            new Row
                            {
                                Children =
                                {
                                    Marker(new Color(124, 205, 255)),
                                    new Offstage
                                    {
                                        IsOffstage = true,
                                        Child = Marker(new Color(255, 111, 97)),
                                    },
                                    Marker(new Color(255, 209, 102)),
                                    new SizedBox { Width = 48 },
                                    new Offstage
                                    {
                                        IsOffstage = false,
                                        Child = Marker(new Color(255, 111, 97)),
                                    },
                                },
                            },
                            new SizedBox { Height = 28 },
                            Label(
                                "IndexedStack — Index の子だけを出す。大きさは一番大きい子に合う"
                            ),
                            new SizedBox { Height = 10 },
                            // 灰色の背景が一番大きい子(120×120)の大きさになり、
                            // その左上に Index = 1 の赤い子(60×60)だけが描かれる。
                            new ColoredBox
                            {
                                Color = new Color(40, 47, 64),
                                Child = new IndexedStack
                                {
                                    Index = 1,
                                    Children =
                                    {
                                        new ColoredBox
                                        {
                                            Color = new Color(124, 205, 255),
                                            Child = new SizedBox { Width = 120, Height = 120 },
                                        },
                                        new ColoredBox
                                        {
                                            Color = new Color(255, 111, 97),
                                            Child = new SizedBox { Width = 60, Height = 60 },
                                        },
                                        new ColoredBox
                                        {
                                            Color = new Color(255, 209, 102),
                                            Child = new SizedBox { Width = 90, Height = 90 },
                                        },
                                    },
                                },
                            },
                            new SizedBox { Height = 28 },
                            Label("RotatedBox — QuarterTurns の数だけ時計回りに90度回す"),
                            new SizedBox { Height = 10 },
                            new Row
                            {
                                CrossAxisAlignment = CrossAxisAlignment.Start,
                                Children =
                                {
                                    // 回転はレイアウトの前に効く。1回と3回では幅と高さが入れ替わる。
                                    new RotatedBox { QuarterTurns = 0, Child = Tag("0回") },
                                    new SizedBox { Width = 32 },
                                    new RotatedBox { QuarterTurns = 1, Child = Tag("1回") },
                                    new SizedBox { Width = 32 },
                                    new RotatedBox { QuarterTurns = 2, Child = Tag("2回") },
                                    new SizedBox { Width = 32 },
                                    new RotatedBox { QuarterTurns = 3, Child = Tag("3回") },
                                },
                            },
                        },
                    },
                },
            },
        };

    private static Widget Marker(Color color) =>
        new ColoredBox
        {
            Color = color,
            Child = new SizedBox { Width = 60, Height = 60 },
        };

    private static Widget Tag(string text) =>
        new ColoredBox
        {
            Color = new Color(124, 205, 255),
            Child = new Padding
            {
                Spacing = EdgeInsets.Symmetric(vertical: 8, horizontal: 16),
                Child = new Text($"QuarterTurns {text}")
                {
                    Style = new TextStyle { FontSize = 20, Color = new Color(16, 20, 31) },
                },
            },
        };

    private static Widget Label(string text) =>
        new Text(text)
        {
            Style = new TextStyle { FontSize = 18, Color = new Color(169, 180, 204) },
        };
}
