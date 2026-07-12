namespace FloatSoda.UI;

/// <summary>
/// ヘッドレスウィジェットが公開するインタラクション状態のスナップショット。
/// デザインシステム層はこの状態を見た目へ純粋にマッピングする
/// (Avalonia の疑似クラス契約の型付き版)。
/// </summary>
public readonly record struct InteractionState
{
    /// <summary>ポインタが押下中かどうか。</summary>
    public bool IsPressed { get; init; }

    /// <summary>ポインタがホバー中かどうか。</summary>
    public bool IsHovered { get; init; }

    /// <summary>フォーカスを持っているかどうか。</summary>
    public bool IsFocused { get; init; }

    /// <summary>操作が無効化されているかどうか。</summary>
    public bool IsDisabled { get; init; }
}
