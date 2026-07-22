using FloatSoda.Abstractions.Geometries;
using FloatSoda.Abstractions.Input;
using FloatSoda.Core;
using FloatSoda.Geometrics;
using FloatSoda.Gesture;
using FloatSoda.RenderObjects;
using FloatSoda.RenderObjects.Layout;
using FloatSoda.RenderObjects.Painting;

namespace FloatSoda.Test.RenderObjects;

public class HitTestTest
{
    private static RenderView Build(RenderBox child)
    {
        var view = new RenderView { Child = child };
        var pipeline = new RenderPipeline
        {
            OnNeedVisualUpdate = () => { },
            RenderView = view
        };
        view.PrepareInitialFrame();
        pipeline.FlushLayout();
        return view;
    }

    private static RenderPointerListener BuildListener() => new()
    {
        Child = new RenderColoredBox
        {
            Child = new RenderConstrainedBox
            {
                AdditionalConstraints = BoxConstraints.Tight(100, 100)
            }
        }
    };

    [Fact]
    public void FlexChild_IsHit_AtItsPaintOffset()
    {
        // 縦Flexの2番目の子(y=100〜200)に配置したListenerへ届くこと
        var listener = BuildListener();
        var flex = new RenderFlex
        {
            Direction = Axis.Vertical,
            MainAxisSize = MainAxisSize.Min,
            Children =
            {
                new RenderConstrainedBox { AdditionalConstraints = BoxConstraints.Tight(100, 100) },
                listener,
            }
        };
        var view = Build(flex);

        var result = new HitTestResult();
        view.HitTest(result, new Offset(50, 150));

        Assert.Contains(result.Path, entry => ReferenceEquals(entry.Target, listener));
    }

    [Fact]
    public void FlexChild_IsNotHit_OutsideItsBounds()
    {
        var listener = BuildListener();
        var flex = new RenderFlex
        {
            Direction = Axis.Vertical,
            MainAxisSize = MainAxisSize.Min,
            Children =
            {
                new RenderConstrainedBox { AdditionalConstraints = BoxConstraints.Tight(100, 100) },
                listener,
            }
        };
        var view = Build(flex);

        // 1番目の子(Listenerなし)の領域
        var result = new HitTestResult();
        view.HitTest(result, new Offset(50, 50));

        Assert.DoesNotContain(result.Path, entry => ReferenceEquals(entry.Target, listener));
    }

    [Fact]
    public void HitPath_IsOrderedInnermostFirst()
    {
        var listener = BuildListener();
        var view = Build(listener);

        var result = new HitTestResult();
        view.HitTest(result, new Offset(50, 50));

        // 最内(RenderColoredBox) → Listener → RenderView の順
        Assert.Collection(
            result.Path,
            entry => Assert.IsType<RenderColoredBox>(entry.Target),
            entry => Assert.Same(listener, entry.Target),
            entry => Assert.Same(view, entry.Target));
    }

    [Fact]
    public void RenderPointerListener_DispatchesEventByPhase()
    {
        var received = new List<PointerEventPhase>();
        var listener = new RenderPointerListener
        {
            OnPointerDown = e => received.Add(e.Phase),
            OnPointerMove = e => received.Add(e.Phase),
            OnPointerUp = e => received.Add(e.Phase),
        };
        var entry = new HitTestEntry(listener);

        listener.HandleEvent(new PointerEvent(1, PointerEventPhase.Down, Offset.Zero), entry);
        listener.HandleEvent(new PointerEvent(1, PointerEventPhase.Move, Offset.Zero), entry);
        listener.HandleEvent(new PointerEvent(1, PointerEventPhase.Up, Offset.Zero), entry);
        listener.HandleEvent(new PointerEvent(1, PointerEventPhase.Add, Offset.Zero), entry);

        Assert.Equal(
            [PointerEventPhase.Down, PointerEventPhase.Move, PointerEventPhase.Up],
            received);
    }
}
