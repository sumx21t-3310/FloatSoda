using FloatSoda.Abstractions.Geometries;
using FloatSoda.Abstractions.Input;

namespace FloatSoda.Gesture;

/// <summary>
/// ドラッグ（パン）を認識する。Down 後にスロップを超えて動いた時点でアリーナへ勝利を宣言し、
/// 以降の移動量を <see cref="OnPanUpdate"/> に delta で通知する。
/// </summary>
public sealed class PanGestureRecognizer : GestureRecognizer
{
    /// <summary>ドラッグ開始（勝利確定）時。引数は開始位置。</summary>
    public Action<Offset>? OnPanStart { get; set; }

    /// <summary>ドラッグ中の移動。引数は前回からの delta。</summary>
    public Action<Offset>? OnPanUpdate { get; set; }

    /// <summary>ドラッグ終了時。</summary>
    public Action? OnPanEnd { get; set; }

    private Offset _initialPosition;
    private Offset _lastPosition;
    private bool _acceptedByArena;
    private bool _started;

    protected override void AddAllowedPointer(PointerEvent downEvent)
    {
        _initialPosition = downEvent.Position;
        _lastPosition = downEvent.Position;
        _acceptedByArena = false;
        _started = false;
    }

    protected override void HandleEvent(PointerEvent pointerEvent)
    {
        switch (pointerEvent.Phase)
        {
            case PointerEventPhase.Move:
                // スロップ超過で初めてドラッグ確定。単独メンバーでも Down 時点では開始しない。
                if (!_started &&
                    Distance(pointerEvent.Position, _initialPosition) > GestureConstants.TouchSlop)
                {
                    if (!_acceptedByArena) Resolve(pointerEvent.PointerId, GestureDisposition.Accepted);

                    if (_acceptedByArena)
                    {
                        _started = true;
                        _lastPosition = _initialPosition;
                        OnPanStart?.Invoke(_initialPosition);
                    }
                }

                if (_started)
                {
                    OnPanUpdate?.Invoke(pointerEvent.Position - _lastPosition);
                    _lastPosition = pointerEvent.Position;
                }
                break;

            case PointerEventPhase.Up:
                if (_started)
                {
                    OnPanEnd?.Invoke();
                    StopTrackingPointer(pointerEvent.PointerId);
                }
                else
                {
                    // 一度も動かず離した＝ドラッグではない。辞退してタップ等に譲る。
                    Resolve(pointerEvent.PointerId, GestureDisposition.Rejected);
                    StopTrackingPointer(pointerEvent.PointerId);
                }
                break;
        }
    }

    // アリーナ勝利は「ドラッグ資格の確定」まで。実際の開始(OnPanStart)はスロップ超過時に行う。
    public override void AcceptGesture(int pointer) => _acceptedByArena = true;

    public override void RejectGesture(int pointer) => StopTrackingPointer(pointer);

    private static double Distance(Offset a, Offset b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return Math.Sqrt((dx * dx) + (dy * dy));
    }
}
