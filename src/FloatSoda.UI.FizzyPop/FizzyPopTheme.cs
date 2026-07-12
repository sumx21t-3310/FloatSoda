using FloatSoda.Elements;
using FloatSoda.Widgets;

namespace FloatSoda.UI.FizzyPop;

/// <summary>
/// FizzyPop デザインシステムのテーマを子孫へ伝播する InheritedWidget。
/// テーマが祖先に存在しなくても各コンポーネントは既定スタイルで動作する。
/// </summary>
public record FizzyPopTheme : InheritedWidget
{
    /// <summary>ボタンの既定スタイル。</summary>
    public ButtonStyle ButtonStyle { get; init; } = ButtonStyle.Default;

    /// <summary>
    /// 最も近い祖先の <see cref="FizzyPopTheme"/> を取得し、依存関係を登録します。
    /// 見つからない場合は null を返します(既定スタイルへのフォールバックは呼び出し側で行う)。
    /// </summary>
    public static FizzyPopTheme? Of(IBuildContext context) =>
        context.DependOnInheritedWidgetOfExactType<FizzyPopTheme>();

    public override bool UpdateShouldNotify(InheritedWidget oldWidget) => !Equals(oldWidget, this);
}
