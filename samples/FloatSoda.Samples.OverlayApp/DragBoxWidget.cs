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
    /// <inheritdoc />
    public override State<DragBoxWidget> CreateState() => new DragBoxState();
}

/// <summary><see cref="DragBoxWidget"/>のドラッグ位置を保持し、表示を構築します。</summary>
public class DragBoxState : State<DragBoxWidget>
{
    /// <summary>ドラッグ領域の一辺の長さ。単位は論理ピクセルです。</summary>
    private const float ContainerSize = 600f;

    /// <summary>ドラッグ対象の一辺の長さ。単位は論理ピクセルです。</summary>
    private const float BoxSize = 140f;

    /// <summary>ドラッグ領域からドラッグ対象を除いた移動可能距離。単位は論理ピクセルです。</summary>
    /// <remarks>移動量を<see cref="Alignment"/>の-1から1の範囲へ変換するために使用します。</remarks>
    private const float Remaining = ContainerSize - BoxSize;

    /// <summary>ドラッグ対象の現在の配置を保持します。</summary>
    private Alignment _alignment = Alignment.Center;

    /// <inheritdoc />
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
