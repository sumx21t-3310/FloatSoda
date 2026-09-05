using FloatSoda.Abstractions.Input;
using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.Painting;
using FloatSoda.Gesture;
using FloatSoda.RenderObjects;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Gesture;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using SkiaSharp;
using PointerRegionWidget = FloatSoda.Widgets.Gesture.PointerRegion;

namespace FloatSoda.Samples.PointerRegion;

/// <summary>
/// Dashboardレーザー入力のEnter、Exit、Cancel、およびTapを目視確認するサンプルです。
/// </summary>
public record PointerRegionDemo : StatefulWidget<PointerRegionDemo>
{
    /// <inheritdoc />
    public override State<PointerRegionDemo> CreateState() => new PointerRegionDemoState();
}

/// <summary><see cref="PointerRegionDemo"/>のイベント状態と表示を管理します。</summary>
public sealed class PointerRegionDemoState : State<PointerRegionDemo>
{
    private bool _hovered;
    private bool _pressed;
    private bool _lastPressCanceled;
    private string _status = "OUTSIDE";
    private int _enterCount;
    private int _exitCount;
    private int _cancelCount;
    private int _tapCount;

    /// <inheritdoc />
    public override Widget Build(IBuildContext context)
    {
        return new SizedBox
        {
            Width = 760,
            Height = 680,
            Child = new ColoredBox
            {
                Color = new SKColor(20, 24, 36),
                Child = new Flex
                {
                    Direction = Axis.Vertical,
                    MainAxisAlignment = MainAxisAlignment.Center,
                    CrossAxisAlignment = CrossAxisAlignment.Center,
                    Children =
                    {
                        BuildText("PointerRegion", 56, SKColors.White, 700),
                        new SizedBox { Height = 12 },
                        BuildText("Aim, press the trigger, then leave while holding.", 24, SKColors.LightGray),
                        new SizedBox { Height = 34 },
                        BuildInteractiveRegion(),
                        new SizedBox { Height = 30 },
                        BuildText($"Status: {_status}", 34, StatusColor, 700),
                        new SizedBox { Height = 18 },
                        BuildText(
                            $"Enter {_enterCount}    Exit {_exitCount}    Cancel {_cancelCount}    Tap {_tapCount}",
                            26,
                            SKColors.Gainsboro),
                    }
                }
            }
        };
    }

    private Widget BuildInteractiveRegion()
    {
        return new PointerRegionWidget
        {
            OnPointerEnter = HandleEnter,
            OnPointerExit = HandleExit,
            Child = new Listener
            {
                Behaviour = HitTestBehaviour.Opaque,
                OnPointerDown = HandleDown,
                OnPointerUp = HandleUp,
                OnPointerCancel = HandleCancel,
                Child = new GestureDetector
                {
                    Behaviour = HitTestBehaviour.Opaque,
                    OnTap = HandleTap,
                    Child = BuildTarget()
                }
            }
        };
    }

    /// <summary>状態に応じて色とラベルが変わる的(まと)を構築します。</summary>
    private Widget BuildTarget()
    {
        return new ClipRoundRect
        {
            BorderRadius = BorderRadius.All(Radius.Circular(28)),
            Child = new SizedBox
            {
                Width = 520,
                Height = 300,
                Child = new ColoredBox
                {
                    Color = RegionColor,
                    Child = new Center
                    {
                        Child = BuildText(RegionLabel, 52, SKColors.White, 700)
                    }
                }
            }
        };
    }

    private SKColor RegionColor => (_pressed, _hovered, _lastPressCanceled) switch
    {
        (true, _, _) => SKColors.DarkOrange,
        (_, true, _) => SKColors.DeepSkyBlue,
        (_, _, true) => SKColors.Crimson,
        _ => SKColors.SlateGray,
    };

    private SKColor StatusColor => _lastPressCanceled ? SKColors.OrangeRed : SKColors.LightSkyBlue;

    private string RegionLabel => _pressed
        ? "PRESSED"
        : _hovered
            ? "HOVER"
            : _lastPressCanceled
                ? "CANCELED"
                : "AIM HERE";

    private void HandleEnter(PointerEvent pointerEvent)
    {
        Log(pointerEvent);
        SetState(() =>
        {
            _hovered = true;
            _enterCount++;
            _status = "ENTER / HOVERING";
        });
    }

    private void HandleExit(PointerEvent pointerEvent)
    {
        Log(pointerEvent);
        SetState(() =>
        {
            _hovered = false;
            _pressed = false;
            _exitCount++;
            _status = _lastPressCanceled ? "EXIT AFTER CANCEL" : "EXIT / OUTSIDE";
        });
    }

    private void HandleDown(PointerEvent pointerEvent)
    {
        Log(pointerEvent);
        SetState(() =>
        {
            _pressed = true;
            _lastPressCanceled = false;
            _status = "DOWN / PRESSED";
        });
    }

    private void HandleUp(PointerEvent pointerEvent)
    {
        Log(pointerEvent);
        SetState(() =>
        {
            _pressed = false;
            _status = "UP / RELEASED";
        });
    }

    private void HandleCancel(PointerEvent pointerEvent)
    {
        Log(pointerEvent);
        SetState(() =>
        {
            _pressed = false;
            _lastPressCanceled = true;
            _cancelCount++;
            _status = "CANCEL / TAP SUPPRESSED";
        });
    }

    private void HandleTap()
    {
        Console.WriteLine("Tap");
        SetState(() =>
        {
            _pressed = false;
            _lastPressCanceled = false;
            _tapCount++;
            _status = "TAP / SUCCESS";
        });
    }

    private static void Log(PointerEvent pointerEvent)
        => Console.WriteLine(
            $"{pointerEvent.Phase,-6} pointer={pointerEvent.PointerId} " +
            $"position=({pointerEvent.Position.X:F1}, {pointerEvent.Position.Y:F1})");

    private static Widget BuildText(string text, float fontSize, SKColor color, int fontWeight = 400)
    {
        return new RichText
        {
            Text = new TextSpan(text)
            {
                Style = new TextStyle
                {
                    Color = color,
                    FontSize = fontSize,
                    FontWeight = fontWeight,
                }
            }
        };
    }
}
