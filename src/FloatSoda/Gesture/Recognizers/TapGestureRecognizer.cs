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

    /// <summary>現在のポインター列で押下が始まった位置。</summary>
    private Offset _initialPosition;

    /// <summary>ジェスチャアリーナがこの認識器を受理したかどうか。</summary>
    private bool _wonArena;

    /// <summary>Upイベントを受信済みかどうか。</summary>
    private bool _receivedUp;

    /// <summary><see cref="OnTap"/>を通知済みかどうか。</summary>
    private bool _fired;

    /// <summary><see cref="OnTapCancel"/>を通知済みかどうか。</summary>
    private bool _canceled;

    /// <inheritdoc />
    protected override void AddAllowedPointer(PointerEvent downEvent)
    {
        _initialPosition = downEvent.Position;
        _wonArena = false;
        _receivedUp = false;
        _fired = false;
        _canceled = false;
        OnTapDown?.Invoke(downEvent.Position);
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public override void AcceptGesture(int pointer)
    {
        _wonArena = true;
        CheckFire(pointer);
    }

    /// <inheritdoc />
    public override void RejectGesture(int pointer) => Cancel(pointer);

    /// <summary>タップを不成立として通知し、対象ポインターの購読を解除します。</summary>
    /// <param name="pointer">購読を解除するポインター識別子。</param>
    /// <remarks>すでに不成立を通知済みの場合は何も行いません。</remarks>
    private void Cancel(int pointer)
    {
        if (_canceled) return;

        _canceled = true;
        OnTapCancel?.Invoke();
        StopTrackingPointer(pointer);
    }

    /// <summary>アリーナ勝利とUp受信がそろった場合にタップ成立を通知します。</summary>
    /// <param name="pointer">成立後に購読を解除するポインター識別子。</param>
    private void CheckFire(int pointer)
    {
        if (_fired || _canceled || !_wonArena || !_receivedUp) return;

        _fired = true;
        OnTap?.Invoke();
        StopTrackingPointer(pointer);
    }

    /// <summary>2点間のユークリッド距離を計算します。</summary>
    /// <param name="a">一方の位置。</param>
    /// <param name="b">もう一方の位置。</param>
    /// <returns>2点間の距離。単位は論理ピクセルです。</returns>
    private static double Distance(Offset a, Offset b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return Math.Sqrt((dx * dx) + (dy * dy));
    }
}
