using FloatSoda.Animation;
using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.RenderObjects;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Animation;
using FloatSoda.Widgets.Components;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using SkiaSharp;
using Topten.RichTextKit;

namespace FloatSoda.Samples.OverlayApp;

/// <summary>
/// AnimationControllerのデモ。呼吸するように不透明度が往復する。
/// SetStateは使わず、FadeTransition(Changed購読→再ペイント)だけで駆動される。
/// </summary>
public record PulseWidget : StatefulWidget<PulseWidget>
{
    public override State<PulseWidget> CreateState() => new PulseState();
}

public record PulseState : TickerProviderState<PulseWidget>
{
    private AnimationController? _opacity;

    public override void InitState()
    {
        _opacity = new AnimationController
        {
            Vsync = this,
            Duration = TimeSpan.FromSeconds(1.5)
        };

        // Completed/Dismissedで折り返して往復させる
        _opacity.StatusChanged += status =>
        {
            switch (status)
            {
                case AnimationStatus.Completed:
                    _opacity.Reverse();
                    break;
                case AnimationStatus.Dismissed:
                    _opacity.Forward();
                    break;
            }
        };

        _opacity.Forward();
    }

    public override Widget Build(IBuildContext context)
    {
        return new FadeTransition
        {
            Opacity = _opacity!,
            Child = new Align
            {
                Alignment = Alignment.Center,
                Child = new SizedBox
                {
                    Width = 400,
                    Height = 200,
                    Child = new ClipRoundRect
                    {
                        BorderRadius = BorderRadius.All(Radius.Circular(60)),
                        Child = new ColoredBox
                        {
                            Color = SKColors.MediumSlateBlue,
                            Child = new Center
                            {
                                Child = new RichText
                                {
                                    Text = new TextSpan("Pulse")
                                    {
                                        Style = new Style
                                        {
                                            TextColor = SKColors.WhiteSmoke,
                                            FontSize = 80
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
