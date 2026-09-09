using FloatSoda.Elements;
using FloatSoda.Painting;

namespace FloatSoda.Widgets;

/// <summary>
/// 子孫の<see cref="Text"/>へ既定のテキスト書式を伝播するInheritedWidgetです。
/// </summary>
/// <remarks>
/// 配下の<see cref="Text"/>は、<see cref="Text.Style"/>で明示されなかったプロパティを
/// この書式から継承します(明示指定 &gt; DefaultTextStyle &gt; フレームワークの既定値)。
/// </remarks>
public sealed record DefaultTextStyle : InheritedWidget
{
    /// <summary>
    /// 子孫へ適用するテキスト書式を取得します。
    /// </summary>
    public required TextStyle Style { get; init; }

    private static readonly DefaultTextStyle Fallback = new()
    {
        Style = new TextStyle(),
        Child = new NullWidget()
    };

    /// <summary>
    /// 最も近い祖先の<see cref="DefaultTextStyle"/>を取得し、
    /// 呼び出し元をそのInheritedWidgetの依存対象として登録します。
    /// </summary>
    /// <param name="context">書式を要求するウィジェットのBuildContext。</param>
    /// <returns>
    /// 最も近い祖先の<see cref="DefaultTextStyle"/>。
    /// 祖先に存在しない場合は、全プロパティが未指定の書式を持つフォールバックを返します。
    /// フォールバックはウィジェットツリーへ組み込めません。
    /// </returns>
    public static DefaultTextStyle Of(IBuildContext context) =>
        context.DependOnInheritedWidgetOfExactType<DefaultTextStyle>() ?? Fallback;

    /// <summary>
    /// テキスト書式が変更され、依存するElementの再ビルドが必要かを判定します。
    /// </summary>
    /// <param name="oldWidget">更新前のInheritedWidget。</param>
    /// <returns>
    /// 更新前のウィジェットが<see cref="DefaultTextStyle"/>であり、
    /// <see cref="Style"/>の値が変更された場合は<see langword="true"/>。
    /// それ以外の場合は<see langword="false"/>。
    /// </returns>
    public override bool UpdateShouldNotify(InheritedWidget oldWidget) =>
        oldWidget is DefaultTextStyle old && old.Style != Style;

    /// <summary>
    /// <see cref="Of"/>のフォールバックが子として保持する、ツリーへ組み込めないウィジェットです。
    /// </summary>
    private sealed record NullWidget : StatelessWidget
    {
        public override Widget Build(IBuildContext context) =>
            throw new InvalidOperationException(
                "DefaultTextStyle.Of(context)が返すフォールバックはウィジェットツリーへ組み込めません。" +
                "ツリーへ配置するDefaultTextStyleは、StyleとChildを指定して生成してください。");
    }
}
