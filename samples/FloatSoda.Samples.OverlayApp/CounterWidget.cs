using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.RenderObjects;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Components;
using FloatSoda.Widgets.Gesture;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using SkiaSharp;
using Topten.RichTextKit;

namespace FloatSoda.Samples.OverlayApp;

/// <summary>
/// シンプルなカウンターアプリ。+/-ボタンでカウントを増減できる。
/// StatefulWidget で状態を保持し、SetState() で UI を更新するデモ。
/// </summary>
public record CounterWidget : StatefulWidget<CounterWidget>
{
    /// <inheritdoc />
    public override State<CounterWidget> CreateState() => new CounterState();
}

/// <summary><see cref="CounterWidget"/>のカウント値を保持し、表示を構築します。</summary>
public class CounterState : State<CounterWidget>
{
    /// <summary>画面に表示する現在のカウント値。</summary>
    private int _count = 0;

    /// <summary>押下時に指定された処理を実行するカウンターボタンを構築します。</summary>
    /// <param name="label">ボタンに表示する文字列。</param>
    /// <param name="onPressed">ボタンがタップされたときに状態更新内で実行する処理。</param>
    /// <returns>構築したボタンウィジェット。</returns>
    private Widget BuildButton(string label, Action onPressed)
    {
        return new GestureDetector
        {
            OnTap = () => SetState(onPressed),
            Child = new ClipRoundRect
            {
                BorderRadius = BorderRadius.All(Radius.Circular(15)),
                Child = new SizedBox
                {
                    Width = 120,
                    Height = 80,
                    Child = new ColoredBox
                    {
                        Color = SKColors.CornflowerBlue,
                        Child = new Center
                        {
                            Child = new RichText
                            {
                                Text = new TextSpan(label)
                                {
                                    Style = new Style
                                    {
                                        TextColor = SKColors.WhiteSmoke,
                                        FontSize = 50,
                                        FontWeight = 700
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };
    }

    /// <inheritdoc />
    public override Widget Build(IBuildContext context)
    {
        return new Align
        {
            Alignment = Alignment.Center,
            Child = new SizedBox
            {
                Width = 600,
                Height = 800,
                Child = new ColoredBox
                {
                    Color = SKColors.Gainsboro,
                    Child = new Flex
                    {
                        Direction = Axis.Vertical,
                        MainAxisAlignment = MainAxisAlignment.Center,
                        CrossAxisAlignment = CrossAxisAlignment.Center,
                        Children =
                        {
                            // タイトル
                            new SizedBox
                            {
                                Height = 80,
                                Child = new Center
                                {
                                    Child = new RichText
                                    {
                                        Text = new TextSpan("Counter")
                                        {
                                            Style = new Style
                                            {
                                                TextColor = SKColors.Black,
                                                FontSize = 60,
                                                FontWeight = 700
                                            }
                                        }
                                    }
                                }
                            },

                            // カウント表示
                            new SizedBox
                            {
                                Width = 400,
                                Height = 200,
                                Child = new ClipRoundRect
                                {
                                    BorderRadius = BorderRadius.All(Radius.Circular(30)),
                                    Child = new ColoredBox
                                    {
                                        Color = SKColors.White,
                                        Child = new Center
                                        {
                                            Child = new RichText
                                            {
                                                Text = new TextSpan(_count.ToString())
                                                {
                                                    Style = new Style
                                                    {
                                                        TextColor = SKColors.DarkSlateGray,
                                                        FontSize = 120,
                                                        FontWeight = 700
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            },

                            // ボタンエリア
                            new SizedBox
                            {
                                Height = 40
                            },

                            new Flex
                            {
                                Direction = Axis.Horizontal,
                                MainAxisAlignment = MainAxisAlignment.Center,
                                CrossAxisAlignment = CrossAxisAlignment.Center,
                                Children =
                                {
                                    BuildButton("-", () => _count--),
                                    new SizedBox { Width = 60 },
                                    BuildButton("+", () => _count++),
                                }
                            },

                            // リセットボタン
                            new SizedBox { Height = 40 },
                            new GestureDetector
                            {
                                OnTap = () => SetState(() => _count = 0),
                                Child = new ClipRoundRect
                                {
                                    BorderRadius = BorderRadius.All(Radius.Circular(15)),
                                    Child = new SizedBox
                                    {
                                        Width = 200,
                                        Height = 70,
                                        Child = new ColoredBox
                                        {
                                            Color = SKColors.Tomato,
                                            Child = new Center
                                            {
                                                Child = new RichText
                                                {
                                                    Text = new TextSpan("Reset")
                                                    {
                                                        Style = new Style
                                                        {
                                                            TextColor = SKColors.White,
                                                            FontSize = 40,
                                                            FontWeight = 700
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };
    }
}
