using FloatSoda.Abstractions.Geometries;
using FloatSoda.Abstractions.Input;
using FloatSoda.Core;
using FloatSoda.Gesture;
using FloatSoda.RenderObjects;
using FloatSoda.Widgets.Gesture;
using FloatSoda.Widgets.Layout;

namespace FloatSoda.Test.Core;

public class PointerInputIntegrationTest
{
    private sealed class FakeRawPointerSource : IRawPointerSource
    {
        public event Action<RawPointerEvent>? OnPointerEvent;

        public void Raise(RawPointerEvent rawEvent) => OnPointerEvent?.Invoke(rawEvent);

        public void Dispose() { }
    }

    [Fact]
    public void LeaveWhilePressed_DispatchesCancelAndExit_ThenNextTapStillWorks()
    {
        var received = new List<string>();
        var binding = new WidgetBinding(new RenderView());
        binding.AttachRootWidget(new PointerRegion
        {
            OnPointerEnter = _ => received.Add("enter"),
            OnPointerExit = _ => received.Add("exit"),
            Child = new Listener
            {
                Behaviour = HitTestBehaviour.Opaque,
                OnPointerDown = _ => received.Add("down"),
                OnPointerUp = _ => received.Add("up"),
                OnPointerCancel = _ => received.Add("cancel"),
                Child = new GestureDetector
                {
                    Behaviour = HitTestBehaviour.Opaque,
                    OnTap = () => received.Add("tap"),
                    Child = new SizedBox
                    {
                        Width = 100,
                        Height = 100,
                    }
                }
            }
        });
        binding.FlushBuildAndLayout();

        using var source = new FakeRawPointerSource();
        using var controller = new PointerController(source);
        controller.OnPointerEvent += binding.HandlePointerEvent;

        source.Raise(new RawPointerEvent(RawPointerKind.Enter, new Offset(10, 10)));
        source.Raise(new RawPointerEvent(RawPointerKind.ButtonDown, new Offset(10, 10)));
        source.Raise(new RawPointerEvent(RawPointerKind.Leave, new Offset(150, 150)));
        source.Raise(new RawPointerEvent(RawPointerKind.ButtonUp, new Offset(150, 150)));
        controller.Flush();

        Assert.Equal(["enter", "down", "cancel", "exit"], received);

        source.Raise(new RawPointerEvent(RawPointerKind.Enter, new Offset(10, 10)));
        source.Raise(new RawPointerEvent(RawPointerKind.ButtonDown, new Offset(10, 10)));
        source.Raise(new RawPointerEvent(RawPointerKind.ButtonUp, new Offset(10, 10)));
        controller.Flush();

        Assert.Equal(
            ["enter", "down", "cancel", "exit", "enter", "down", "up", "tap"],
            received);
    }
}
