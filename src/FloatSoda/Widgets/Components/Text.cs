using FloatSoda.Elements;
using FloatSoda.Painting;
using FloatSoda.RenderObjects;

namespace FloatSoda.Widgets.Components;

/// <summary>
/// 書式付きテキストを段落としてレイアウトし、描画するウィジェットです。
/// </summary>
/// <seealso cref="RenderParagraph"/>
public sealed record RichText : MultiChildRenderObjectWidget<RenderParagraph>
{
    /// <summary>
    /// 段落に表示する書式付きテキストを取得します。
    /// </summary>
    public required TextSpan Text { get; init; }

    /// <summary>
    /// このウィジェットのテキストを描画するRenderObjectを生成します。
    /// </summary>
    /// <returns>指定された書式付きテキストを保持する新しいRenderObject。</returns>
    public override RenderParagraph CreateRenderObject() => new() { Text = Text };

    /// <summary>
    /// 表示する書式付きテキストを既存のRenderObjectへ反映します。
    /// </summary>
    /// <param name="renderObject">このウィジェットに対応するRenderObject。</param>
    /// <remarks>
    /// テキストが変更された場合、対象をLayout Dirtyとしてマークし、
    /// 次のパイプライン更新時に段落のサイズを再計算します。
    /// 値が変更されなかった場合、Dirty状態は変更されません。
    /// </remarks>
    public override void UpdateRenderObject(RenderParagraph renderObject) => renderObject.Text = Text;
}

/// <summary>
/// 単一の書式で文字列を表示するウィジェットです。
/// </summary>
/// <seealso cref="RichText"/>
public sealed record Text : StatelessWidget
{
    /// <summary>
    /// 表示する文字列を指定してウィジェットを初期化します。
    /// </summary>
    /// <param name="data">表示する文字列。空文字列も指定できます。</param>
    /// <exception cref="ArgumentNullException"><paramref name="data"/>が<see langword="null"/>です。</exception>
    public Text(string data)
    {
        ArgumentNullException.ThrowIfNull(data);
        Data = data;
    }

    /// <summary>表示する文字列を取得します。</summary>
    /// <exception cref="ArgumentNullException">値が<see langword="null"/>です。</exception>
    public string Data
    {
        get;
        init
        {
            ArgumentNullException.ThrowIfNull(value, nameof(Data));
            field = value;
        }
    }

    /// <summary>文字列に適用する書式を取得します。</summary>
    /// <remarks><see langword="null"/>の場合は段落の既定書式を使用します。</remarks>
    public TextStyle? Style { get; init; }

    /// <summary>
    /// 文字列を段落として描画する子ウィジェットを構築します。
    /// </summary>
    /// <param name="context">このウィジェットが配置されている構築コンテキスト。</param>
    /// <returns><see cref="Data"/>を表示する<see cref="RichText"/>。</returns>
    public override Widget Build(IBuildContext context)
    {
        var text = new TextSpan(Data) { Style = Style };

        return new RichText
        {
            Text = text
        };
    }
}
