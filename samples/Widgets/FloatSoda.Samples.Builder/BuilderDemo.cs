using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.Gesture;
using FloatSoda.Painting;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Gesture;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using BuilderWidget = FloatSoda.Widgets.Builder;

namespace FloatSoda.Samples.Builder;

/// <summary><see cref="BuilderWidget"/>、<see cref="KeyedSubtree"/>、<see cref="RepaintBoundary"/>の働きを確認するサンプルです。</summary>
public sealed record BuilderDemo : StatefulWidget<BuilderDemo>
{
    /// <inheritdoc />
    public override State<BuilderDemo> CreateState() => new BuilderDemoState();
}

/// <summary><see cref="BuilderDemo"/>のカウンターの並び順を管理します。</summary>
public sealed class BuilderDemoState : State<BuilderDemo>
{
    private static readonly Color Blue = new(124, 205, 255);
    private static readonly Color Red = new(255, 111, 97);

    private bool _swapped;

    /// <inheritdoc />
    public override Widget Build(IBuildContext context) =>
        new SizedBox
        {
            Width = 940,
            Height = 520,
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
                            Label(
                                "Builder — 同じ Build の中で、すぐ上のウィジェットを context から読む"
                            ),
                            new SizedBox { Height = 10 },
                            new DefaultTextStyle
                            {
                                Style = new TextStyle
                                {
                                    FontSize = 26,
                                    Color = new Color(255, 209, 102),
                                },
                                Child = new Row
                                {
                                    Children =
                                    {
                                        // Build の context は DefaultTextStyle より上にあるので、この書式を見つけられない。
                                        Value(
                                            $"Build の context: FontSize {DescribeFontSize(context)}"
                                        ),
                                        new SizedBox { Width = 32 },
                                        // Builder が渡す context は DefaultTextStyle の下にあるので、26 を読める。
                                        new BuilderWidget
                                        {
                                            ChildBuilder = innerContext =>
                                                Value(
                                                    $"Builder の context: FontSize {DescribeFontSize(innerContext)}"
                                                ),
                                        },
                                    },
                                },
                            },
                            new SizedBox { Height = 36 },
                            Label(
                                "KeyedSubtree — Key を付けると、並べ替えても状態が子に付いていく"
                            ),
                            new SizedBox { Height = 10 },
                            SwapButton(),
                            new SizedBox { Height = 16 },
                            // Key なし: 入れ替えると色だけが入れ替わり、回数は左右の位置に残る。
                            new Row
                            {
                                Children =
                                [
                                    Caption("Key なし"),
                                    .. Ordered(
                                        new TapCounter { Color = Blue, Name = "青" },
                                        new TapCounter { Color = Red, Name = "赤" }
                                    ),
                                ],
                            },
                            new SizedBox { Height = 16 },
                            // Key あり: 入れ替えると、回数も色と一緒に動く。
                            new Row
                            {
                                Children =
                                [
                                    Caption("Key あり"),
                                    .. Ordered(
                                        new KeyedSubtree
                                        {
                                            Key = new ValueKey<string>("blue"),
                                            Child = new TapCounter { Color = Blue, Name = "青" },
                                        },
                                        new KeyedSubtree
                                        {
                                            Key = new ValueKey<string>("red"),
                                            Child = new TapCounter { Color = Red, Name = "赤" },
                                        }
                                    ),
                                ],
                            },
                            new SizedBox { Height = 36 },
                            Label("RepaintBoundary — 描き直す範囲を分ける。見た目は変わらない"),
                            new SizedBox { Height = 10 },
                            new Row
                            {
                                Children =
                                {
                                    Swatch("なし"),
                                    new SizedBox { Width = 24 },
                                    // 子を別のレイヤーへ描く。親が描き直されても、この子は描き直さずに済む。
                                    new RepaintBoundary { Child = Swatch("あり") },
                                },
                            },
                        },
                    },
                },
            },
        };

    private IEnumerable<Widget> Ordered(Widget first, Widget second) =>
        _swapped ? [second, Gap(), first] : [first, Gap(), second];

    private Widget SwapButton() =>
        new GestureDetector
        {
            Behaviour = HitTestBehaviour.Opaque,
            OnTap = () => SetState(() => _swapped = !_swapped),
            Child = new ColoredBox
            {
                Color = new Color(90, 98, 117),
                Child = new Padding
                {
                    Spacing = EdgeInsets.Symmetric(vertical: 8, horizontal: 16),
                    Child = new Text("左右を入れ替える")
                    {
                        Style = new TextStyle { FontSize = 20, Color = new Color(244, 247, 255) },
                    },
                },
            },
        };

    private static string DescribeFontSize(IBuildContext context) =>
        DefaultTextStyle.Of(context).Style.FontSize?.ToString() ?? "指定なし";

    private static Widget Gap() => new SizedBox { Width = 16 };

    private static Widget Value(string text) => new Text(text);

    private static Widget Caption(string text) =>
        new SizedBox
        {
            Width = 120,
            Child = new Text(text)
            {
                Style = new TextStyle { FontSize = 18, Color = new Color(169, 180, 204) },
            },
        };

    private static Widget Swatch(string text) =>
        new ColoredBox
        {
            Color = new Color(40, 47, 64),
            Child = new Padding
            {
                Spacing = EdgeInsets.Symmetric(vertical: 12, horizontal: 20),
                Child = new Text(text)
                {
                    Style = new TextStyle { FontSize = 20, Color = new Color(244, 247, 255) },
                },
            },
        };

    private static Widget Label(string text) =>
        new Text(text)
        {
            Style = new TextStyle { FontSize = 18, Color = new Color(169, 180, 204) },
        };
}

/// <summary>タップした回数を自分の状態として持つ四角です。</summary>
public sealed record TapCounter : StatefulWidget<TapCounter>
{
    /// <summary>四角の色を取得します。</summary>
    public required Color Color { get; init; }

    /// <summary>四角に出す名前を取得します。</summary>
    public required string Name { get; init; }

    /// <inheritdoc />
    public override State<TapCounter> CreateState() => new TapCounterState();
}

/// <summary><see cref="TapCounter"/>のタップ回数を管理します。</summary>
public sealed class TapCounterState : State<TapCounter>
{
    private int _count;

    /// <inheritdoc />
    public override Widget Build(IBuildContext context) =>
        new GestureDetector
        {
            Behaviour = HitTestBehaviour.Opaque,
            OnTap = () => SetState(() => _count++),
            Child = new ColoredBox
            {
                Color = Widget!.Color,
                Child = new SizedBox
                {
                    Width = 160,
                    Height = 56,
                    Child = new Center
                    {
                        Child = new Text($"{Widget.Name} {_count} 回")
                        {
                            Style = new TextStyle { FontSize = 20, Color = new Color(16, 20, 31) },
                        },
                    },
                },
            },
        };
}
