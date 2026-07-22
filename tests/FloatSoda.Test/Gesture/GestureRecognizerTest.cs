using FloatSoda.Abstractions.Geometries;
using FloatSoda.Abstractions.Input;
using FloatSoda.Gesture;

namespace FloatSoda.Test.Gesture;

public class GestureRecognizerTest
{
    /// <summary>WidgetBinding のディスパッチ順(Listener→Router+Arena)を模したドライバ。</summary>
    private sealed class Harness
    {
        public GestureArenaManager Arena { get; } = new();
        public PointerRouter Router { get; } = new();

        private readonly List<GestureRecognizer> _recognizers = [];

        public T Add<T>(T recognizer) where T : GestureRecognizer
        {
            recognizer.Bind(Arena, Router);
            _recognizers.Add(recognizer);
            return recognizer;
        }

        public void Down(Offset position, int pointer = 1)
        {
            var e = new PointerEvent(pointer, PointerEventPhase.Down, position);
            foreach (var r in _recognizers) r.AddPointer(e);   // Listener 相当
            Router.Route(e);                                    // WidgetBinding 相当
            Arena.Close(pointer);
        }

        public void Move(Offset position, int pointer = 1)
            => Router.Route(new PointerEvent(pointer, PointerEventPhase.Move, position));

        public void Up(Offset position, int pointer = 1)
        {
            Router.Route(new PointerEvent(pointer, PointerEventPhase.Up, position));
            Arena.Sweep(pointer);
        }
    }

    [Fact]
    public void Tap_Fires_WhenPressedAndReleasedInPlace()
    {
        var harness = new Harness();
        var tapped = 0;
        harness.Add(new TapGestureRecognizer { OnTap = () => tapped++ });

        harness.Down(new Offset(10, 10));
        harness.Up(new Offset(11, 11));

        Assert.Equal(1, tapped);
    }

    [Fact]
    public void Tap_Cancels_WhenMovedBeyondSlop()
    {
        var harness = new Harness();
        var tapped = 0;
        var canceled = 0;
        harness.Add(new TapGestureRecognizer { OnTap = () => tapped++, OnTapCancel = () => canceled++ });

        harness.Down(new Offset(10, 10));
        harness.Move(new Offset(10, 40));   // 30px > slop(18)
        harness.Up(new Offset(10, 40));

        Assert.Equal(0, tapped);
        Assert.Equal(1, canceled);
    }

    [Fact]
    public void Pan_WinsOverTap_WhenDragged()
    {
        var harness = new Harness();
        var tapped = 0;
        var panStarted = 0;
        var totalDelta = Offset.Zero;

        harness.Add(new TapGestureRecognizer { OnTap = () => tapped++ });
        harness.Add(new PanGestureRecognizer
        {
            OnPanStart = _ => panStarted++,
            OnPanUpdate = d => totalDelta += d,
        });

        harness.Down(new Offset(0, 0));
        harness.Move(new Offset(0, 30));    // slop 超過 → pan 勝利、tap 敗北
        harness.Move(new Offset(0, 50));
        harness.Up(new Offset(0, 50));

        Assert.Equal(0, tapped);
        Assert.Equal(1, panStarted);
        Assert.Equal(50, totalDelta.Y, precision: 3);
    }

    [Fact]
    public void Tap_WinsOverPan_WhenTappedInPlace()
    {
        var harness = new Harness();
        var tapped = 0;
        var panStarted = 0;

        harness.Add(new TapGestureRecognizer { OnTap = () => tapped++ });
        harness.Add(new PanGestureRecognizer { OnPanStart = _ => panStarted++ });

        harness.Down(new Offset(5, 5));
        harness.Up(new Offset(6, 5));       // ほぼ動かず離す → tap 勝利

        Assert.Equal(1, tapped);
        Assert.Equal(0, panStarted);
    }
}
