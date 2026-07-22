using FloatSoda.Abstractions.Geometries;
using FloatSoda.Abstractions.Input;

namespace FloatSoda.Gesture;

/// <summary>
/// タップ（Down → スロップ内で静止 → Up）を認識する。Down 後にスロップを超えて動くと辞退し、
/// Up まで残ってアリーナに勝てば <see cref="OnTap"/> を発火する。
/// </summary>
public sealed class TapGestureRecognizer : GestureRecognizer
{
    /// <summary>タップが成立したとき。</summary>
    public Action? OnTap { get; set; }

    /// <summary>押下位置を伴う Down 通知。</summary>
    public Action<Offset>? OnTapDown { get; set; }

    /// <summary>離した位置を伴う Up 通知（成立前でも発火する）。</summary>
    public Action<Offset>? OnTapUp { get; set; }

    /// <summary>タップが不成立に終わったとき（他ジェスチャに敗北 / スロップ超過）。</summary>
    public Action? OnTapCancel { get; set; }

    private Offset _initialPosition;
    private bool _wonArena;
    private bool _receivedUp;
    private bool _fired;
    private bool _canceled;

    protected override void AddAllowedPointer(PointerEvent downEvent)
    {
        _initialPosition = downEvent.Position;
        _wonArena = false;
        _receivedUp = false;
        _fired = false;
        _canceled = false;
        OnTapDown?.Invoke(downEvent.Position);
    }

    protected override void HandleEvent(PointerEvent pointerEvent)
    {
        switch (pointerEvent.Phase)
        {
            case PointerEventPhase.Move:
                if (Distance(pointerEvent.Position, _initialPosition) > GestureConstants.TouchSlop)
                {
                    // スロップ超過はタップではない。アリーナへ辞退しつつ、既に単独勝利済みでも
                    // 自前でキャンセルして Up での発火を抑止する。
                    Resolve(pointerEvent.PointerId, GestureDisposition.Rejected);
                    Cancel(pointerEvent.PointerId);
                }
                break;

            case PointerEventPhase.Up:
                _receivedUp = true;
                OnTapUp?.Invoke(pointerEvent.Position);
                Resolve(pointerEvent.PointerId, GestureDisposition.Accepted);
                CheckFire(pointerEvent.PointerId);
                break;
        }
    }

    public override void AcceptGesture(int pointer)
    {
        _wonArena = true;
        CheckFire(pointer);
    }

    public override void RejectGesture(int pointer) => Cancel(pointer);

    private void Cancel(int pointer)
    {
        if (_canceled) return;

        _canceled = true;
        OnTapCancel?.Invoke();
        StopTrackingPointer(pointer);
    }

    private void CheckFire(int pointer)
    {
        if (_fired || _canceled || !_wonArena || !_receivedUp) return;

        _fired = true;
        OnTap?.Invoke();
        StopTrackingPointer(pointer);
    }

    private static double Distance(Offset a, Offset b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return Math.Sqrt((dx * dx) + (dy * dy));
    }
}
