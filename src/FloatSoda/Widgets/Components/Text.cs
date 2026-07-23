using FloatSoda.Elements;
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
/// <param name="Data">表示する文字列。<see langword="null"/>は指定できません。</param>
/// <seealso cref="RichText"/>
public sealed record Text(string Data) : StatelessWidget
{
    /// <summary>
    /// 文字列を段落として描画する子ウィジェットを構築します。
    /// </summary>
    /// <param name="context">このウィジェットが配置されている構築コンテキスト。</param>
    /// <returns><see cref="Data"/>を表示する<see cref="RichText"/>。</returns>
    public override Widget Build(IBuildContext context)
    {
        var text = new TextSpan(Data);

        return new RichText
        {
            Text = text
        };
    }
}