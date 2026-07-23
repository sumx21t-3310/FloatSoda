namespace FloatSoda.Widgets;

/// <summary>
/// Widgetツリーの差分更新でウィジェットを識別するキーのマーカーインターフェースです。
/// </summary>
public interface IKey;

/// <summary>
/// 値の等価性によってウィジェットを識別するキーです。
/// 同じ型引数と等しい値を持つキーは、同じウィジェットを示します。
/// </summary>
/// <typeparam name="T">ウィジェットの識別に使用する値の型。</typeparam>
/// <param name="Value">ウィジェットを識別する値。</param>
public record struct ValueKey<T>(T Value) : IKey;

/// <summary>
/// GUIDによってウィジェットを識別するキーです。
/// 既定の初期化で生成した各インスタンスは、異なるウィジェットを示します。
/// </summary>
public record struct UniqueKey() : IKey
{
    /// <summary>
    /// キーの等価性判定に使用する識別子を取得します。
    /// 値を明示しない場合は、新しいインスタンスの生成時に一意なGUIDが割り当てられます。
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();
}