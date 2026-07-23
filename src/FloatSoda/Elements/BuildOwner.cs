using FloatSoda.Core;
using FloatSoda.Gesture;

namespace FloatSoda.Elements;

/// <summary>
/// 1つのElementツリーについて、再構築が必要なElementの予約と実行順序を管理します。
/// </summary>
/// <param name="onBuildScheduled">
/// 再構築待ちリストが空の状態から最初のElementが予約されたときに呼び出す処理。
/// </param>
public class BuildOwner(Action onBuildScheduled)
{
    /// <summary>
    /// このツリーが属するウィンドウのフレームスケジューラ(通常はWidgetBinding)。
    /// TickerがElement.Owner経由で自ウィンドウのフレームコールバックへ到達するために使います。
    /// </summary>
    public IFrameScheduler? FrameScheduler { get; init; }

    /// <summary>
    /// このツリーが属するウィンドウのジェスチャ調停基盤(通常はWidgetBinding)。
    /// GestureDetectorがElement.Owner経由で共有のアリーナ／ルータへ到達するために使います。
    /// </summary>
    public IGestureBinding? GestureBinding { get; init; }

    private readonly List<Element> _dirtyElements = [];
    private bool? _dirtyElementsNeedsRestoring;
    private bool _scheduledFlushDirtyElements;

    /// <summary>
    /// 指定したElementを次のビルドスコープで再構築する対象として登録します。
    /// </summary>
    /// <param name="element">再構築が必要なElement。</param>
    /// <remarks>
    /// Elementがまだ待ちリストにない場合だけ追加します。
    /// 最初のElementを追加すると、コンストラクターで指定されたビルド予約処理を呼び出します。
    /// </remarks>
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

    /// <summary>
    /// 任意の処理を実行した後、予約されたElementを親に近い順で再構築します。
    /// </summary>
    /// <param name="callback">
    /// 再構築待ちリストを処理する前に実行する処理。追加処理が不要な場合は<see langword="null"/>。
    /// </param>
    /// <remarks>
    /// 処理中に新たな再構築が予約された場合も順序を再評価して同じスコープ内で処理します。
    /// 完了後は、処理したElementの待ちリスト登録状態を解除します。
    /// </remarks>
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