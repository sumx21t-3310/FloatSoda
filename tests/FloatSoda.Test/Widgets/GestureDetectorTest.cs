using FloatSoda.Abstractions.Geometries;
using FloatSoda.Abstractions.Input;
using FloatSoda.Core;
using FloatSoda.Gesture;
using FloatSoda.RenderObjects;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Gesture;
using FloatSoda.Widgets.Layout;

namespace FloatSoda.Test.Widgets;

public class GestureDetectorTest
{
    [Fact]
    public void Dispose_押下中にツリーから外す_離してもOnTapを呼ばない()
    {
        var log = new List<string>();
        var binding = Mount(new GestureDetector
        {
            Behaviour = HitTestBehaviour.Opaque,
            OnTap = () => log.Add("tap"),
            Child = new SizedBox { Width = 100, Height = 100 },
        });

        Send(binding, PointerEventPhase.Add, 10, 10);
        Send(binding, PointerEventPhase.Down, 10, 10);

        binding.AttachRootWidget(new SizedBox { Width = 100, Height = 100 });
        binding.FlushBuildAndLayout();

        Send(binding, PointerEventPhase.Up, 10, 10);

        Assert.Empty(log);
    }

    [Fact]
    public void Dispose_入れ子で押下中に内側を外す_外側のOnTapを呼ぶ()
    {
        var log = new List<string>();
        var binding = Mount(Nested(withInner: true, log));

        Send(binding, PointerEventPhase.Add, 10, 10);
        Send(binding, PointerEventPhase.Down, 10, 10);

        binding.AttachRootWidget(Nested(withInner: false, log));
        binding.FlushBuildAndLayout();

        Send(binding, PointerEventPhase.Up, 10, 10);

        Assert.Equal(["outer-tap"], log);
    }

    [Fact]
    public void Dispose_押下中にツリーから外す_次の押下では新しいウィジェットだけが反応する()
    {
        var log = new List<string>();
        var binding = Mount(new GestureDetector
        {
            Behaviour = HitTestBehaviour.Opaque,
            OnTap = () => log.Add("old-tap"),
            Child = new SizedBox { Width = 100, Height = 100 },
        });

        Send(binding, PointerEventPhase.Add, 10, 10);
        Send(binding, PointerEventPhase.Down, 10, 10);

        binding.AttachRootWidget(new SizedBox
        {
            Width = 100,
            Height = 100,
            Child = new GestureDetector
            {
                Behaviour = HitTestBehaviour.Opaque,
                OnTap = () => log.Add("new-tap"),
                Child = new SizedBox { Width = 100, Height = 100 },
            },
        });
        binding.FlushBuildAndLayout();

        Send(binding, PointerEventPhase.Up, 10, 10);
        Send(binding, PointerEventPhase.Down, 10, 10);
        Send(binding, PointerEventPhase.Up, 10, 10);

        Assert.Equal(["new-tap"], log);
    }

    private static Widget Nested(bool withInner, List<string> log) => new GestureDetector
    {
        Behaviour = HitTestBehaviour.Opaque,
        OnTap = () => log.Add("outer-tap"),
        Child = withInner
            ? new GestureDetector
            {
                Behaviour = HitTestBehaviour.Opaque,
                OnTap = () => log.Add("inner-tap"),
                Child = new SizedBox { Width = 100, Height = 100 },
            }
            : new SizedBox { Width = 100, Height = 100 },
    };

    private static WidgetBinding Mount(Widget root)
    {
        var binding = new WidgetBinding(new RenderView());
        binding.AttachRootWidget(root);
        binding.FlushBuildAndLayout();
        return binding;
    }

    private static void Send(WidgetBinding binding, PointerEventPhase phase, double x, double y)
        => binding.HandlePointerEvent(new PointerEvent(1, phase, new Offset(x, y)));
}
