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
/// ボックスを掴んでドラッグで動かせるデモ。
/// GestureDetector.OnPanUpdate（Pan 認識器）で移動量を受け取り、
/// Align.Alignment を更新して位置を反映する StatefulWidget の例。
/// </summary>
public record DragBoxWidget : StatefulWidget<DragBoxWidget>
{
    public override State<DragBoxWidget> CreateState() => new DragBoxState();
}

public class DragBoxState : State<DragBoxWidget>
{
    private const float ContainerSize = 600f;
    private const float BoxSize = 140f;

    // Align は子を remaining(= Container - Box) の範囲に -1..1 で配置する。
    // よって delta(px) → alignment の変換係数は 2 / remaining。
    private const float Remaining = ContainerSize - BoxSize;

    private Alignment _alignment = Alignment.Center;

    public override Widget Build(IBuildContext context)
    {
        return new Align
        {
            Alignment = Alignment.Center,
            Child = new SizedBox
            {
                Width = ContainerSize,
                Height = ContainerSize,
                Child = new ColoredBox
                {
                    Color = SKColors.Gainsboro,
                    Child = new Align
                    {
                        Alignment = _alignment,
                        Child = new GestureDetector
                        {
                            OnPanUpdate = delta => SetState(() =>
                            {
                                var nx = Math.Clamp(_alignment.X + ((float)delta.X * 2f / Remaining), -1f, 1f);
                                var ny = Math.Clamp(_alignment.Y + ((float)delta.Y * 2f / Remaining), -1f, 1f);
                                _alignment = new Alignment(nx, ny);
                            }),
                            Child = new ClipRoundRect
                            {
                                BorderRadius = BorderRadius.All(Radius.Circular(20)),
                                Child = new SizedBox
                                {
                                    Width = BoxSize,
                                    Height = BoxSize,
                                    Child = new ColoredBox
                                    {
                                        Color = SKColors.MediumSlateBlue,
                                        Child = new Center
                                        {
                                            Child = new RichText
                                            {
                                                Text = new TextSpan("Drag")
                                                {
                                                    Style = new Style
                                                    {
                                                        TextColor = SKColors.WhiteSmoke,
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
        };
    }
}
