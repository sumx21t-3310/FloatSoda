namespace FloatSoda.Gesture;

/// <summary>
/// ウィンドウ単位のジェスチャ調停基盤。<see cref="GestureArenaManager"/> と
/// <see cref="PointerRouter"/> を保持し、認識器がこれ経由で共有のアリーナ／ルータへ到達する。
/// FloatSoda では <c>WidgetBinding</c> が実装する。
/// </summary>
public interface IGestureBinding
{
    /// <summary>このウィンドウのジェスチャアリーナ。</summary>
    GestureArenaManager GestureArena { get; }

    /// <summary>このウィンドウのポインタルータ。</summary>
    PointerRouter PointerRouter { get; }
}
