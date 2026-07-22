using System.Collections.Concurrent;
using System.Diagnostics;
using FloatSoda.Abstractions.Geometries;

namespace FloatSoda.Abstractions.Input;

/// <summary>
/// <see cref="IRawPointerSource"/> の生イベント列を <see cref="PointerEvent"/>（Add/Remove 合成・PointerId 採番済み）へ変換する状態機械。
/// 生イベントは任意のスレッドから受け取りキューに積むだけで、変換と <see cref="OnPointerEvent"/> の発火は
/// <see cref="Flush"/> を呼んだスレッド（UI スレッドを想定）上で行います。
/// </summary>
public class PointerController : IDisposable
{
    private bool _pointerCurrentlyAdded = false;
    private bool _pointerCurrentlyDown = false;
    private bool _buttonPressed = false;
    private int _pointerId = 0;

    private readonly ConcurrentQueue<RawPointerEvent> _pendingEvents = new();

    private readonly IRawPointerSource _pointerSource;

    /// <summary>変換済みポインタイベントの通知。<see cref="Flush"/> を呼んだスレッドで発火します。</summary>
    public event Action<PointerEvent>? OnPointerEvent;

    public PointerController(IRawPointerSource pointerSource)
    {
        _pointerSource = pointerSource;
        pointerSource.OnPointerEvent += Enqueue;
    }

    private void Enqueue(RawPointerEvent rawEvent) => _pendingEvents.Enqueue(rawEvent);

    /// <summary>
    /// キューに溜まった生イベントを取り出して状態機械へ流し、<see cref="OnPointerEvent"/> を発火します。
    /// UI スレッドから毎フレーム呼びます。
    /// </summary>
    public void Flush()
    {
        while (_pendingEvents.TryDequeue(out var rawEvent))
        {
            Handle(rawEvent);
        }
    }

    private void Handle(RawPointerEvent rawEvent)
    {
        switch (rawEvent.Kind)
        {
            case RawPointerKind.Enter:
                SendPointerEvent(PointerEventPhase.Add, rawEvent.Position);
                break;
            case RawPointerKind.Leave:
                SendPointerEvent(PointerEventPhase.Remove, rawEvent.Position);
                break;
            case RawPointerKind.Move:
                OnPointerMoved(rawEvent.Position);
                break;
            case RawPointerKind.ButtonDown:
            case RawPointerKind.ButtonUp:
                OnPointerButton(rawEvent);
                break;
            case RawPointerKind.Scroll:
            default:
                break;
        }
    }

    private void OnPointerMoved(Offset position)
    {
        if (!_buttonPressed && !_pointerCurrentlyDown) return;

        var phase = (_buttonPressed, _pointerCurrentlyDown) switch
        {
            (true, true) => PointerEventPhase.Move,
            (true, false) => PointerEventPhase.Down,
            (false, true) => PointerEventPhase.Up,
            _ => throw new UnreachableException()
        };

        SendPointerEvent(phase, position);
    }

    private void OnPointerButton(RawPointerEvent rawEvent)
    {
        if (rawEvent.Button != PointerButton.Left) return;
        _buttonPressed = rawEvent.Kind == RawPointerKind.ButtonDown;

        if (!_buttonPressed && !_pointerCurrentlyDown) return;
        var phase = (_buttonPressed, _pointerCurrentlyDown) switch
        {
            (true, true) => PointerEventPhase.Move,
            (true, false) => PointerEventPhase.Down,
            (false, true) => PointerEventPhase.Up,
            _ => throw new UnreachableException()
        };

        SendPointerEvent(phase, rawEvent.Position);
    }


    private void SendPointerEvent(PointerEventPhase phase, Offset offset)
    {
        if (!_pointerCurrentlyAdded && phase != PointerEventPhase.Add)
        {
            SendPointerEvent(PointerEventPhase.Add, offset);
        }

        if (_pointerCurrentlyAdded && phase == PointerEventPhase.Add)
        {
            return;
        }

        if (phase == PointerEventPhase.Add)
        {
            _pointerId++;
        }

        OnPointerEvent?.Invoke(new PointerEvent(_pointerId, phase, offset));

        switch (phase)
        {
            case PointerEventPhase.Add:
                _pointerCurrentlyAdded = true;
                break;
            case PointerEventPhase.Remove:
                _pointerCurrentlyAdded = false;
                break;
            case PointerEventPhase.Down:
                _pointerCurrentlyDown = true;
                break;
            case PointerEventPhase.Up:
                _pointerCurrentlyDown = false;
                break;
            case PointerEventPhase.Move:
            default:
                break;
        }
    }

    public void Dispose()
    {
        _pointerSource.OnPointerEvent -= Enqueue;
    }
}
