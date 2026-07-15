using FloatSoda.Abstractions.Geometries;
using FloatSoda.Abstractions.Input;

namespace FloatSoda.Gesture;

public interface IHitTestTarget
{
    void HandleEvent(PointerEvent pointerEvent, HitTestEntry entry);
}

public readonly record struct HitTestEntry(IHitTestTarget Target, Offset? Transform = null);

public class HitTestResult
{
    public List<HitTestEntry> Path { get; private set; } = [];

    private readonly List<Offset> _transforms = [Offset.Zero];

    private readonly List<Offset> _localTransform = [];


    private void GlobalizeTransform()
    {
        if (_localTransform.Count == 0) return;

        var last = _transforms[^1];

        foreach (var part in _localTransform)
        {
            last = part + last;
            _transforms.Add(last);
        }

        _localTransform.Clear();
    }


}
