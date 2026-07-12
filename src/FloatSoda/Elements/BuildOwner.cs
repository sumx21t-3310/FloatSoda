using FloatSoda.Core;

namespace FloatSoda.Elements;

public class BuildOwner(Action onBuildScheduled)
{
    /// <summary>
    /// このツリーが属するウィンドウのフレームスケジューラ(通常はWidgetBinding)。
    /// TickerがElement.Owner経由で自ウィンドウのフレームコールバックへ到達するために使います。
    /// </summary>
    public IFrameScheduler? FrameScheduler { get; init; }

    private readonly List<Element> _dirtyElements = [];
    private bool? _dirtyElementsNeedsRestoring;
    private bool _scheduledFlushDirtyElements;

    public void ScheduledBuildFor(Element element)
    {
        if (element.InDirtyList)
        {
            _dirtyElementsNeedsRestoring = true;
            return;
        }

        if (!_scheduledFlushDirtyElements)
        {
            onBuildScheduled();
        }
        
        _dirtyElements.Add(element);
        element.InDirtyList = true;
    }

    public void BuildScope(Action? callback = null)
    {
        if (callback == null && _dirtyElements.Count == 0) return;

        _scheduledFlushDirtyElements = true;

        if (callback != null)
        {
            _dirtyElementsNeedsRestoring = false;
            callback();
        }
        
        _dirtyElements.Sort();

        _dirtyElementsNeedsRestoring = false;

        var dirtyCount = _dirtyElements.Count;
        var index = 0;

        while (index < dirtyCount)
        {
            var element = _dirtyElements[index];
            element.Rebuild();
            index++;

            if (dirtyCount < _dirtyElements.Count || (_dirtyElementsNeedsRestoring ?? false))
            {
                _dirtyElements.Sort();
                _dirtyElementsNeedsRestoring = false;
                dirtyCount = _dirtyElements.Count;
                while (index > 0 && _dirtyElements[index -1].Dirty)
                {
                    index--;
                }
            }
        }

        foreach (var dirtyElement in _dirtyElements)
        {
            dirtyElement.InDirtyList = false;
        }
        
        _dirtyElements.Clear();

        _scheduledFlushDirtyElements = false;
        _dirtyElementsNeedsRestoring = null;
    }
}