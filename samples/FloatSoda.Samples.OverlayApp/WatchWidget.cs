using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.RenderObjects;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Components;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using SkiaSharp;
using Topten.RichTextKit;


namespace FloatSoda.Samples.OverlayApp;

public record WatchWidget : StatefulWidget<WatchWidget>
{
    public override State<WatchWidget> CreateState() => new WatchState();
}

public class WatchState : State<WatchWidget>
{
    private Timer? _timer;
    private string _time = "00:00:00";

    public override void InitState()
    {
        _timer = new Timer(UpdateTime, null, dueTime: 0, period: 1000);
    }

    private void UpdateTime(object? _) => SetState(() => _time = DateTime.Now.ToString("HH:mm:ss"));

    public override Widget Build(IBuildContext context)
    {
        return new Align
        {
            Alignment = Alignment.Center,
            Child = new SizedBox
            {
                Width = 500,
                Height = 500,
                Child = new ClipRoundRect
                {
                    BorderRadius = BorderRadius.All(Radius.Circular(200)),
                    Child = new ColoredBox
                    {
                        Color = SKColors.WhiteSmoke,
                        Child = new Center()
                        {
                            Child = new RichText
                            {
                                Text = new TextSpan(_time)
                                {
                                    Style = new Style
                                    {
                                        TextColor = SKColor.FromHsv(0, 0, 10),
                                        FontSize = 100
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