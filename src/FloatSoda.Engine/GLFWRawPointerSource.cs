using FloatSoda.Abstractions.Geometries;
using FloatSoda.Abstractions.Input;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace FloatSoda.Engine;

/// <summary>
/// GLFW ウィンドウのマウス入力を <see cref="RawPointerEvent"/> に変換する入力源。
/// イベントは GLFW をポンプしているスレッド（レンダースレッド）で発火します。
/// </summary>
public sealed unsafe class GLFWRawPointerSource : IRawPointerSource
{
    private readonly Window* _handle;
    private Offset _currentPosition;

    // GLFW へ渡すデリゲートは GC に回収されないようフィールドで保持する
    private readonly GLFWCallbacks.CursorEnterCallback _cursorEnterCallback;
    private readonly GLFWCallbacks.CursorPosCallback _cursorPosCallback;
    private readonly GLFWCallbacks.MouseButtonCallback _mouseButtonCallback;

    /// <inheritdoc />
    public event Action<RawPointerEvent>? OnPointerEvent;

    public GLFWRawPointerSource(Window* handle)
    {
        if (handle is null)
        {
            throw new ArgumentNullException(nameof(handle));
        }

        _handle = handle;

        // 最初のイベントが届く前の位置も取得しておく
        GLFW.GetCursorPos(_handle, out var x, out var y);
        _currentPosition = new Offset(x, y);

        _cursorEnterCallback = (_, entered) =>
            OnPointerEvent?.Invoke(new RawPointerEvent(
                entered ? RawPointerKind.Enter : RawPointerKind.Leave,
                _currentPosition));

        _cursorPosCallback = (_, cursorX, cursorY) =>
        {
            var position = new Offset(cursorX, cursorY);

            _currentPosition = position;
            OnPointerEvent?.Invoke(new RawPointerEvent(RawPointerKind.Move, position));
        };

        _mouseButtonCallback = (_, button, action, _) =>
        {
            if (TryMapButton(button) is not { } pointerButton) return;
            if (action == InputAction.Repeat) return;

            var kind = action == InputAction.Press ? RawPointerKind.ButtonDown : RawPointerKind.ButtonUp;

            OnPointerEvent?.Invoke(new RawPointerEvent(kind, _currentPosition, pointerButton));
        };

        GLFW.SetCursorEnterCallback(_handle, _cursorEnterCallback);
        GLFW.SetCursorPosCallback(_handle, _cursorPosCallback);
        GLFW.SetMouseButtonCallback(_handle, _mouseButtonCallback);
    }

    private static PointerButton? TryMapButton(MouseButton button) => button switch
    {
        MouseButton.Left => PointerButton.Left,
        MouseButton.Middle => PointerButton.Middle,
        MouseButton.Right => PointerButton.Right,
        _ => null,
    };

    public void Dispose()
    {
        GLFW.SetCursorEnterCallback(_handle, null);
        GLFW.SetCursorPosCallback(_handle, null);
        GLFW.SetMouseButtonCallback(_handle, null);
    }
}
