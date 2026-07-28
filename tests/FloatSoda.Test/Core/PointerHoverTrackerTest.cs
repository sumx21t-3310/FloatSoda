using FloatSoda.Abstractions.Geometries;
using FloatSoda.Abstractions.Input;
using FloatSoda.Core;
using FloatSoda.Gesture;
using FloatSoda.RenderObjects.Gesture;

namespace FloatSoda.Test.Core;

public class PointerHoverTrackerTest
{
    [Fact]
    public void Update_DispatchesEnterOnlyOnce_ThenExitOnRemoval()
    {
        var phases = new List<PointerEventPhase>();
        var listener = new RenderPointerListener
        {
            OnPointerEnter = e => phases.Add(e.Phase),
            OnPointerExit = e => phases.Add(e.Phase),
        };
        var result = BuildResult(listener);
        var tracker = new PointerHoverTracker();

        tracker.Update(new PointerEvent(1, PointerEventPhase.Add, new Offset(10, 10)), result);
        tracker.Update(new PointerEvent(1, PointerEventPhase.Move, new Offset(20, 20)), result);
        tracker.Update(new PointerEvent(1, PointerEventPhase.Remove, new Offset(30, 30)), null);

        Assert.Equal([PointerEventPhase.Enter, PointerEventPhase.Exit], phases);
    }

    [Fact]
    public void Update_WhenPathChanges_DispatchesExitBeforeEnter()
    {
        var calls = new List<string>();
        var first = new RenderPointerListener
        {
            OnPointerEnter = _ => calls.Add("first enter"),
            OnPointerExit = _ => calls.Add("first exit"),
        };
        var second = new RenderPointerListener
        {
            OnPointerEnter = _ => calls.Add("second enter"),
            OnPointerExit = _ => calls.Add("second exit"),
        };
        var tracker = new PointerHoverTracker();

        tracker.Update(new PointerEvent(1, PointerEventPhase.Add, Offset.Zero), BuildResult(first));
        calls.Clear();
        tracker.Update(new PointerEvent(1, PointerEventPhase.Move, Offset.Zero), BuildResult(second));

        Assert.Equal(["first exit", "second enter"], calls);
    }

    [Fact]
    public void Update_PreservesSharedAncestorAcrossSiblingChange()
    {
        var calls = new List<string>();
        var ancestor = new RenderPointerListener
        {
            OnPointerEnter = _ => calls.Add("ancestor enter"),
            OnPointerExit = _ => calls.Add("ancestor exit"),
        };
        var first = new RenderPointerListener
        {
            OnPointerEnter = _ => calls.Add("first enter"),
            OnPointerExit = _ => calls.Add("first exit"),
        };
        var second = new RenderPointerListener
        {
            OnPointerEnter = _ => calls.Add("second enter"),
            OnPointerExit = _ => calls.Add("second exit"),
        };
        var tracker = new PointerHoverTracker();

        tracker.Update(new PointerEvent(1, PointerEventPhase.Add, Offset.Zero), BuildResult(first, ancestor));
        calls.Clear();
        tracker.Update(new PointerEvent(1, PointerEventPhase.Move, Offset.Zero), BuildResult(second, ancestor));

        Assert.Equal(["first exit", "second enter"], calls);
    }

    private static HitTestResult BuildResult(params RenderPointerListener[] listeners)
    {
        var result = new HitTestResult();
        foreach (var listener in listeners)
        {
            result.Add(new HitTestEntry(listener));
        }

        return result;
    }
}
