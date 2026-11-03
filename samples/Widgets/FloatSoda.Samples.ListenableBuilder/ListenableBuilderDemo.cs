using System.ComponentModel;
using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.Gesture;
using FloatSoda.Painting;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Gesture;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using ListenableBuilderWidget = FloatSoda.Widgets.ListenableBuilder;

namespace FloatSoda.Samples.ListenableBuilder;

/// <summary>モデルの変更通知を受けて、<see cref="ListenableBuilderWidget"/>の中だけを作り直す様子を確認するサンプルです。</summary>
public sealed record ListenableBuilderDemo : StatefulWidget<ListenableBuilderDemo>
{
    /// <inheritdoc />
    public override State<ListenableBuilderDemo> CreateState() => new ListenableBuilderDemoState();
}

/// <summary><see cref="ListenableBuilderDemo"/>が使うモデルを持ちます。</summary>
public sealed class ListenableBuilderDemoState : State<ListenableBuilderDemo>
{
    // モデルは State が持つ。Build のたびに作り直すと、購読先が毎回替わってしまう。
    private readonly CounterModel _counter = new();

    /// <inheritdoc />
    public override Widget Build(IBuildContext context) =>
        new SizedBox
        {
            Width = 820,
            Height = 420,
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
                                "ボタンを押すとモデルの Count が増え、PropertyChanged が通知される"
                            ),
                            new SizedBox { Height = 10 },
                            // SetState は呼ばない。モデルを変えるだけで、聞いている ListenableBuilder が作り直される。
                            new GestureDetector
                            {
                                Behaviour = HitTestBehaviour.Opaque,
                                OnTap = () => _counter.Count++,
                                Child = new ColoredBox
                                {
                                    Color = new Color(124, 205, 255),
                                    Child = new Padding
                                    {
                                        Spacing = EdgeInsets.Symmetric(
                                            vertical: 12,
                                            horizontal: 20
                                        ),
                                        Child = new Text("Count を増やす")
                                        {
                                            Style = new TextStyle
                                            {
                                                FontSize = 22,
                                                Color = new Color(16, 20, 31),
                                            },
                                        },
                                    },
                                },
                            },
                            new SizedBox { Height = 32 },
                            Label("ListenableBuilder の中 — 押すたびに数字が変わる"),
                            new SizedBox { Height = 10 },
                            new ListenableBuilderWidget
                            {
                                Listenable = _counter,
                                ChildBuilder = _ =>
                                    Value($"Count: {_counter.Count}", new Color(255, 209, 102)),
                            },
                            new SizedBox { Height = 16 },
                            // 同じモデルを、別の ListenableBuilder からも聞ける。
                            new ListenableBuilderWidget
                            {
                                Listenable = _counter,
                                ChildBuilder = _ =>
                                    Value(
                                        _counter.Count % 2 == 0 ? "偶数" : "奇数",
                                        _counter.Count % 2 == 0
                                            ? new Color(124, 205, 255)
                                            : new Color(255, 111, 97)
                                    ),
                            },
                            new SizedBox { Height = 32 },
                            Label(
                                "ListenableBuilder の外 — State.Build で読んだ値なので、0 のまま変わらない"
                            ),
                            new SizedBox { Height = 10 },
                            Value($"Count: {_counter.Count}", new Color(169, 180, 204)),
                        },
                    },
                },
            },
        };

    private static Widget Value(string text, Color color) =>
        new Text(text)
        {
            Style = new TextStyle { FontSize = 30, Color = color },
        };

    private static Widget Label(string text) =>
        new Text(text)
        {
            Style = new TextStyle { FontSize = 18, Color = new Color(169, 180, 204) },
        };
}

/// <summary>押された回数を持ち、変わったときに<see cref="PropertyChanged"/>で知らせるモデルです。</summary>
public sealed class CounterModel : INotifyPropertyChanged
{
    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>押された回数を取得または設定します。</summary>
    public int Count
    {
        get;
        set
        {
            if (field == value)
                return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
        }
    }
}
