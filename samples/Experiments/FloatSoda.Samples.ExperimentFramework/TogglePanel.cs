using FloatSoda.Elements;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using SkiaSharp;

namespace FloatSoda.Samples.ExperimentFramework;

record TogglePanel : StatefulWidget<TogglePanel>
{
    public bool UseBoundary { get; init; } = true;
    public Action<TogglePanelState>? OnStateCreated { get; init; }

    public override State<TogglePanel> CreateState() => new TogglePanelState();
}

class TogglePanelState : State<TogglePanel>
{
    private bool _flag;

    // SetState は protected なので、外から呼べるように State を受け渡す
    public override void InitState() => Widget!.OnStateCreated?.Invoke(this);

    public void Toggle() => SetState(() => _flag = !_flag);

    private Widget Panel(SKColor color) =>
        new SizedBox
        {
            Width = 100,
            Height = 100,
            Child = new ColoredBox { Color = color },
        };

    private Widget Wrap(Widget child) =>
        Widget!.UseBoundary ? new RepaintBoundary { Child = child } : child;

    public override Widget Build(IBuildContext context) =>
        new Column
        {
            Children =
            {
                Wrap(Panel(_flag ? SKColors.Tomato : SKColors.CornflowerBlue)),
                Wrap(Panel(SKColors.DarkSeaGreen)),
            },
        };
}
