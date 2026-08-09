namespace FloatSoda.Geometrics;

/// <summary>入力ボックスを出力ボックスへ収める方法を指定します。</summary>
/// <remarks><see cref="FloatSoda.Widgets.Layout.FittedBox"/>と今後の画像ウィジェットで共通して使用します。</remarks>
public enum BoxFit
{
    /// <summary>入力の縦横比を変えて出力ボックス全体を埋めます。</summary>
    Fill,

    /// <summary>入力全体が収まる範囲で縦横比を維持して最大化します。</summary>
    Contain,

    /// <summary>出力全体を覆う範囲で縦横比を維持して最小化します。</summary>
    Cover,

    /// <summary>入力の幅全体が表示されるように縦横比を維持して拡大縮小します。</summary>
    FitWidth,

    /// <summary>入力の高さ全体が表示されるように縦横比を維持して拡大縮小します。</summary>
    FitHeight,

    /// <summary>入力を拡大縮小せずに配置し、出力に収まらない部分を除外します。</summary>
    None,

    /// <summary>入力が出力より大きい場合だけ、全体が収まるように縮小します。</summary>
    ScaleDown,
}
