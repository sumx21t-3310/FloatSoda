using FloatSoda.Abstractions.Geometries;
using FloatSoda.Abstractions.Input;

namespace FloatSoda.Gesture;

/// <summary>
/// ドラッグ（パン）を認識する。Down 後にスロップを超えて動いた時点でアリーナへ勝利を宣言し、
/// 以降の移動量を<see cref="OnPanUpdate"/>へ前回位置との差分で通知する。
/// </summary>
public sealed class PanGestureRecognizer : GestureRecognizer
{
    /// <summary>ドラッグ開始（勝利確定）時。引数は開始位置。</summary>
    public Action<Offset>? OnPanStart { get; set; }

    /// <summary>ドラッグ中に、前回位置からの移動量を通知します。</summary>
    public Action<Offset>? OnPanUpdate { get; set; }

    /// <summary>ドラッグ終了時。</summary>
    public Action? OnPanEnd { get; set; }

    /// <summary>現在のポインター列で押下が始まった位置。</summary>
    private Offset _initialPosition;

    /// <summary>直前に移動量を通知した位置。</summary>
    private Offset _lastPosition;

    /// <summary>ジェスチャアリーナがこの認識器を受理したかどうか。</summary>
    private bool _acceptedByArena;

    /// <summary>移動量がしきい値を超え、ドラッグ開始を通知済みかどうか。</summary>
    private bool _started;

    /// <inheritdoc />
    protected override void AddAllowedPointer(PointerEvent downEvent)
    {
        _initialPosition = downEvent.Position;
        _lastPosition = downEvent.Position;
        _acceptedByArena = false;
        _started = false;
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    /// <remarks>
    /// アリーナでの勝利はドラッグ資格だけを確定します。
    /// <see cref="OnPanStart"/>は移動量がしきい値を超えた時点で通知します。
    /// </remarks>
    public override void AcceptGesture(int pointer) => _acceptedByArena = true;

    /// <inheritdoc />
    /// <remarks>対象ポインターのイベント購読を解除します。</remarks>
    public override void RejectGesture(int pointer) => StopTrackingPointer(pointer);

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
