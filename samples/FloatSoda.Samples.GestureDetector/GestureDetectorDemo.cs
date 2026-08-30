using FloatSoda.Abstractions.Geometries;
using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.Gesture;
using FloatSoda.Painting;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Gesture;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using GestureDetectorWidget = FloatSoda.Widgets.Gesture.GestureDetector;

namespace FloatSoda.Samples.GestureDetector;

/// <summary>タップの成立と、ドラッグ(Pan)による移動量の受け取りを確認するサンプルです。</summary>
public sealed record GestureDetectorDemo : StatefulWidget<GestureDetectorDemo>
{
    /// <inheritdoc />
    public override State<GestureDetectorDemo> CreateState() => new GestureDetectorDemoState();
}

/// <summary><see cref="GestureDetectorDemo"/>のタップ回数とドラッグ位置を管理します。</summary>
public sealed class GestureDetectorDemoState : State<GestureDetectorDemo>
{
    private const double FieldWidth = 560;
    private const double FieldHeight = 200;
    private const double BoxSize = 64;

    private int _tapCount;
    private bool _dragging;
    private double _boxX = (FieldWidth - BoxSize) / 2;
    private double _boxY = (FieldHeight - BoxSize) / 2;

    /// <inheritdoc />
    public override Widget Build(IBuildContext context) => new SizedBox
    {
        Width = 700,
        Height = 460,
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
                        Label("OnTap — 押して離すとタップが成立する"),
                        new SizedBox { Height = 10 },
                        // Opaque で領域全体を掴む。押して同じ場所で離すと OnTap が呼ばれる。
                        new GestureDetectorWidget
                        {
                            Behaviour = HitTestBehaviour.Opaque,
                            OnTap = HandleTap,
                            Child = new ColoredBox
                            {
                                Color = new Color(124, 205, 255),
                                Child = new SizedBox
                                {
                                    Width = 220,
                                    Height = 64,
                                    Child = new Center
                                    {
                                        Child = new Text($"タップ {_tapCount} 回")
                                        {
                                            Style = new TextStyle { FontSize = 20, Color = new Color(16, 20, 31) }
                                        }
                                    }
                                }
                            }
                        },
                        new SizedBox { Height = 28 },

                        Label("OnPan — ドラッグで箱を動かす"),
                        new SizedBox { Height = 10 },
                        // OnPanUpdate は前回位置からの移動量(デルタ)で届く。
                        // 累積して Positioned の座標へ反映し、フィールド内へ収める。
                        new ColoredBox
                        {
                            Color = new Color(40, 47, 64),
                            Child = new SizedBox
                            {
                                Width = FieldWidth,
                                Height = FieldHeight,
                                Child = new Stack
                                {
                                    Children =
                                    {
                                        new Positioned
                                        {
                                            Left = _boxX,
                                            Top = _boxY,
                                            Child = new GestureDetectorWidget
                                            {
                                                Behaviour = HitTestBehaviour.Opaque,
                                                OnPanStart = _ => SetState(() => _dragging = true),
                                                OnPanUpdate = HandlePanUpdate,
                                                OnPanEnd = () => SetState(() => _dragging = false),
                                                Child = new ColoredBox
                                                {
                                                    Color = _dragging
                                                        ? new Color(255, 209, 102)
                                                        : new Color(255, 111, 97),
                                                    Child = new SizedBox { Width = BoxSize, Height = BoxSize }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        new SizedBox { Height = 16 },
                        Label($"箱の位置: ({_boxX:F0}, {_boxY:F0})  ドラッグ中: {(_dragging ? "はい" : "いいえ")}")
                    }
                }
            }
        }
    };

    private void HandleTap() => SetState(() => _tapCount++);

    private void HandlePanUpdate(Offset delta) => SetState(() =>
    {
        _boxX = Math.Clamp(_boxX + delta.X, 0, FieldWidth - BoxSize);
        _boxY = Math.Clamp(_boxY + delta.Y, 0, FieldHeight - BoxSize);
    });

    private static Widget Label(string text) => new Text(text)
    {
        Style = new TextStyle { FontSize = 18, Color = new Color(169, 180, 204) }
    };
}
