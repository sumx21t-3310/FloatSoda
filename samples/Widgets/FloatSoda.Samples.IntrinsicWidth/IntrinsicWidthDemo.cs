using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.Painting;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using IntrinsicWidthWidget = FloatSoda.Widgets.Layout.IntrinsicWidth;

namespace FloatSoda.Samples.IntrinsicWidth;

/// <summary>子の本来の幅・高さ(intrinsic サイズ)に揃える<see cref="IntrinsicWidthWidget"/>と<see cref="IntrinsicHeight"/>を確認するサンプルです。</summary>
public sealed record IntrinsicWidthDemo : StatelessWidget
{
    /// <inheritdoc />
    public override Widget Build(IBuildContext context) =>
        new SizedBox
        {
            Width = 940,
            Height = 530,
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
                            Label("IntrinsicWidth — Stretch の Column を、一番長い子の幅に揃える"),
                            new SizedBox { Height = 10 },
                            new Row
                            {
                                CrossAxisAlignment = CrossAxisAlignment.Start,
                                Children =
                                {
                                    // IntrinsicWidth なし: Stretch は親の幅(320)いっぱいまで広がる。
                                    new SizedBox { Width = 320, Child = Menu() },
                                    new SizedBox { Width = 40 },
                                    // IntrinsicWidth あり: 3つとも、一番長い「設定を開く」の幅になる。
                                    new IntrinsicWidthWidget { Child = Menu() },
                                    new SizedBox { Width = 40 },
                                    // StepWidth を指定すると、幅を 100 の倍数へ切り上げる。
                                    new IntrinsicWidthWidget { StepWidth = 100, Child = Menu() },
                                },
                            },
                            new SizedBox { Height = 36 },
                            Label("IntrinsicHeight — Stretch の Row を、一番高い子の高さに揃える"),
                            new SizedBox { Height = 10 },
                            new Row
                            {
                                CrossAxisAlignment = CrossAxisAlignment.Start,
                                Children =
                                {
                                    // IntrinsicHeight なし: Stretch は親の高さ(200)いっぱいまで伸びる。
                                    new SizedBox { Height = 200, Child = Card() },
                                    new SizedBox { Width = 40 },
                                    // IntrinsicHeight あり: 左の帯が、右の2行のテキストと同じ高さになる。
                                    new IntrinsicHeight { Child = Card() },
                                },
                            },
                        },
                    },
                },
            },
        };

    private static Widget Menu() =>
        new Column
        {
            CrossAxisAlignment = CrossAxisAlignment.Stretch,
            Children =
            {
                MenuItem("戻る"),
                new SizedBox { Height = 8 },
                MenuItem("設定を開く"),
                new SizedBox { Height = 8 },
                MenuItem("終了"),
            },
        };

    private static Widget MenuItem(string text) =>
        new ColoredBox
        {
            Color = new Color(124, 205, 255),
            Child = new Padding
            {
                Spacing = EdgeInsets.Symmetric(vertical: 8, horizontal: 12),
                Child = new Text(text)
                {
                    Style = new TextStyle { FontSize = 22, Color = new Color(16, 20, 31) },
                },
            },
        };

    private static Widget Card() =>
        new Row
        {
            CrossAxisAlignment = CrossAxisAlignment.Stretch,
            Children =
            {
                // 高さを持たない帯。Stretch によって Row の高さまで伸びる。
                new ColoredBox
                {
                    Color = new Color(255, 111, 97),
                    Child = new SizedBox { Width = 16 },
                },
                new ColoredBox
                {
                    Color = new Color(40, 47, 64),
                    Child = new Padding
                    {
                        Spacing = EdgeInsets.All(12),
                        Child = new Column
                        {
                            CrossAxisAlignment = CrossAxisAlignment.Start,
                            Children =
                            {
                                new Text("フレンドがオンラインになりました")
                                {
                                    Style = new TextStyle
                                    {
                                        FontSize = 20,
                                        Color = new Color(244, 247, 255),
                                    },
                                },
                                new Text("2分前")
                                {
                                    Style = new TextStyle
                                    {
                                        FontSize = 16,
                                        Color = new Color(169, 180, 204),
                                    },
                                },
                            },
                        },
                    },
                },
            },
        };

    private static Widget Label(string text) =>
        new Text(text)
        {
            Style = new TextStyle { FontSize = 18, Color = new Color(169, 180, 204) },
        };
}
