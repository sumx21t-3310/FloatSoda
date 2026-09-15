using FloatSoda.Abstractions.Input;
using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.Gesture;
using FloatSoda.Painting;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Gesture;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using ListenerWidget = FloatSoda.Widgets.Gesture.Listener;

namespace FloatSoda.Samples.Listener;

/// <summary>低レベルのポインターイベントの通知と、HitTestBehaviour による反応範囲の違いを確認するサンプルです。</summary>
public sealed record ListenerDemo : StatefulWidget<ListenerDemo>
{
    /// <inheritdoc />
    public override State<ListenerDemo> CreateState() => new ListenerDemoState();
}

/// <summary><see cref="ListenerDemo"/>のイベント記録と表示を管理します。</summary>
public sealed class ListenerDemoState : State<ListenerDemo>
{
    private string _lastEvent = "まだイベントはありません";
    private int _downCount;
    private int _upCount;

    /// <inheritdoc />
    public override Widget Build(IBuildContext context) => new SizedBox
    {
        Width = 700,
        Height = 400,
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
                        Label("押す場所と HitTestBehaviour — 反応する範囲が変わる"),
                        new SizedBox { Height = 10 },
                        new Row
                        {
                            Children =
                            {
                                // 既定の DeferToChild は、子(中央の四角)がヒットした時だけ反応する。
                                // 枠の空白部分を押してもイベントは来ない。
                                Pad("DeferToChild", new ListenerWidget
                                {
                                    OnPointerDown = e => Record("DeferToChild", e),
                                    OnPointerUp = e => RecordUp("DeferToChild", e),
                                    Child = new Center
                                    {
                                        Child = Marker(new Color(124, 205, 255))
                                    }
                                }),
                                new SizedBox { Width = 24 },

                                // Opaque は子の空白を含む領域全体が対象になる。
                                // 枠のどこを押してもイベントが来る。
                                Pad("Opaque", new ListenerWidget
                                {
                                    Behaviour = HitTestBehaviour.Opaque,
                                    OnPointerDown = e => Record("Opaque", e),
                                    OnPointerUp = e => RecordUp("Opaque", e),
                                    Child = new Center
                                    {
                                        Child = Marker(new Color(255, 111, 97))
                                    }
                                })
                            }
                        },
                        new SizedBox { Height = 24 },
                        Label($"最後のイベント: {_lastEvent}"),
                        new SizedBox { Height = 8 },
                        Label($"Down {_downCount} 回 / Up {_upCount} 回")
                    }
                }
            }
        }
    };

    private void Record(string name, PointerEvent pointerEvent) => SetState(() =>
    {
        _downCount++;
        _lastEvent = Describe(name, pointerEvent);
    });

    private void RecordUp(string name, PointerEvent pointerEvent) => SetState(() =>
    {
        _upCount++;
        _lastEvent = Describe(name, pointerEvent);
    });

    private static string Describe(string name, PointerEvent pointerEvent) =>
        $"{name} で {pointerEvent.Phase}(位置 {pointerEvent.Position.X:F0}, {pointerEvent.Position.Y:F0})";

    /// <summary>ラベル付きの枠を作り、その中で Listener の反応範囲を見せる。</summary>
    private static Widget Pad(string label, Widget child) => new Column
    {
        MainAxisSize = MainAxisSize.Min,
        Children =
        {
            new ColoredBox
            {
                Color = new Color(40, 47, 64),
                Child = new SizedBox
                {
                    Width = 280,
                    Height = 150,
                    Child = child
                }
            },
            new SizedBox { Height = 10 },
            Label(label)
        }
    };

    private static Widget Label(string text) => new Text(text)
    {
        Style = new TextStyle { FontSize = 18, Color = new Color(169, 180, 204) }
    };

    private static Widget Marker(Color color) => new ColoredBox
    {
        Color = color,
        Child = new SizedBox { Width = 48, Height = 48 }
    };
}
