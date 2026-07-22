using FloatSoda.Abstractions.Geometries;
using FloatSoda.Abstractions.Input;

namespace FloatSoda.Test.Input;

public class PointerControllerTest
{
    private sealed class FakeRawPointerSource : IRawPointerSource
    {
        public event Action<RawPointerEvent>? OnPointerEvent;

        public void Raise(RawPointerEvent rawEvent) => OnPointerEvent?.Invoke(rawEvent);

        public void Dispose() { }
    }

    private static (FakeRawPointerSource Source, PointerController Controller, List<PointerEvent> Events) Build()
    {
        var source = new FakeRawPointerSource();
        var controller = new PointerController(source);
        var events = new List<PointerEvent>();
        controller.OnPointerEvent += events.Add;
        return (source, controller, events);
    }

    [Fact]
    public void EnterDownMoveUpLeave_EmitsAddDownMoveUpRemove()
    {
        var (source, controller, events) = Build();

        source.Raise(new RawPointerEvent(RawPointerKind.Enter, new Offset(10, 20)));
        source.Raise(new RawPointerEvent(RawPointerKind.ButtonDown, new Offset(10, 20)));
        source.Raise(new RawPointerEvent(RawPointerKind.Move, new Offset(30, 40)));
        source.Raise(new RawPointerEvent(RawPointerKind.ButtonUp, new Offset(30, 40)));
        source.Raise(new RawPointerEvent(RawPointerKind.Leave, new Offset(30, 40)));
        controller.Flush();

        Assert.Equal(
            [
                PointerEventPhase.Add,
                PointerEventPhase.Down,
                PointerEventPhase.Move,
                PointerEventPhase.Up,
                PointerEventPhase.Remove,
            ],
            events.Select(e => e.Phase));
        Assert.All(events, e => Assert.Equal(1, e.PointerId));
        Assert.Equal(new Offset(30, 40), events[2].Position);
    }

    [Fact]
    public void DownWithoutEnter_SynthesizesAdd()
    {
        var (source, controller, events) = Build();

        source.Raise(new RawPointerEvent(RawPointerKind.ButtonDown, new Offset(5, 5)));
        controller.Flush();

        Assert.Equal(
            [PointerEventPhase.Add, PointerEventPhase.Down],
            events.Select(e => e.Phase));
        Assert.All(events, e => Assert.Equal(new Offset(5, 5), e.Position));
    }

    [Fact]
    public void NonLeftButton_IsIgnored()
    {
        var (source, controller, events) = Build();

        source.Raise(new RawPointerEvent(RawPointerKind.ButtonDown, new Offset(5, 5), PointerButton.Right));
        source.Raise(new RawPointerEvent(RawPointerKind.ButtonUp, new Offset(5, 5), PointerButton.Middle));
        controller.Flush();

        Assert.Empty(events);
    }

    [Fact]
    public void MoveWithoutPress_IsSuppressed()
    {
        var (source, controller, events) = Build();

        source.Raise(new RawPointerEvent(RawPointerKind.Enter, new Offset(0, 0)));
        source.Raise(new RawPointerEvent(RawPointerKind.Move, new Offset(10, 10)));
        controller.Flush();

        Assert.Equal([PointerEventPhase.Add], events.Select(e => e.Phase));
    }

    [Fact]
    public void PointerId_Increments_AfterRemoveAndReenter()
    {
        var (source, controller, events) = Build();

        source.Raise(new RawPointerEvent(RawPointerKind.Enter, new Offset(0, 0)));
        source.Raise(new RawPointerEvent(RawPointerKind.Leave, new Offset(0, 0)));
        source.Raise(new RawPointerEvent(RawPointerKind.Enter, new Offset(0, 0)));
        controller.Flush();

        Assert.Equal([1, 1, 2], events.Select(e => e.PointerId));
    }

    [Fact]
    public void RawEvents_AreQueuedUntilFlush()
    {
        var (source, controller, events) = Build();

        source.Raise(new RawPointerEvent(RawPointerKind.Enter, new Offset(0, 0)));

        Assert.Empty(events);

        controller.Flush();

        Assert.Single(events);
    }

    [Fact]
    public void Dispose_StopsReceivingRawEvents()
    {
        var (source, controller, events) = Build();

        controller.Dispose();
        source.Raise(new RawPointerEvent(RawPointerKind.Enter, new Offset(0, 0)));
        controller.Flush();

        Assert.Empty(events);
    }
}
