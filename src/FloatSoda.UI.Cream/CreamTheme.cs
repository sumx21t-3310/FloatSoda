using FloatSoda.Elements;
using FloatSoda.Widgets;

namespace FloatSoda.UI.Cream;

/// <summary>
/// Cream デザインシステムのテーマを子孫へ伝播する InheritedWidget。
/// テーマが祖先に存在しなくても各コンポーネントは既定スタイルで動作する。
/// </summary>
public record CreamTheme : InheritedWidget
{
    /// <summary>ボタンの既定スタイル。</summary>
    public ButtonStyle ButtonStyle { get; init; } = ButtonStyle.Default;

    /// <summary>
    /// 最も近い祖先の <see cref="CreamTheme"/> を取得し、依存関係を登録します。
    /// 見つからない場合は null を返します(既定スタイルへのフォールバックは呼び出し側で行う)。
    /// </summary>
    public static CreamTheme? Of(IBuildContext context) =>
        context.DependOnInheritedWidgetOfExactType<CreamTheme>();

    public override bool UpdateShouldNotify(InheritedWidget oldWidget) => !Equals(oldWidget, this);
}
