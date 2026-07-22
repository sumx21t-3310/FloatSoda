using System.Diagnostics;
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
    public IReadOnlyList<HitTestEntry> Path => _pathInternal;

    private readonly List<HitTestEntry> _pathInternal = [];

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

    public Offset LastTransform
    {
        get
        {
            GlobalizeTransform();
            return _transforms[^1];
        }
    }


    public void PushOffset(Offset offset) => _localTransform.Add(offset);

    public void PopTransform()
    {
        if (_localTransform.Count != 0)
        {
            _localTransform.RemoveAt(_localTransform.Count - 1);
        }
        else
        {
            _transforms.RemoveAt(_transforms.Count - 1);
            Debug.Assert(_transforms.Count != 0);
        }
    }

    public void Add(HitTestEntry entry) => _pathInternal.Add(entry with { Transform = LastTransform });

    public bool AddWidthPaintOffset(Offset? offset, Offset position, Func<HitTestResult, Offset, bool> hitTest)
    {
        var transform = offset is not null ? position - offset : position;

        if (offset is not null) PushOffset(-offset.Value);

        var isHit = hitTest(this, (Offset)transform);

        if (offset is not null) PopTransform();

        return isHit;
    }
}
