using FloatSoda.Elements;

namespace FloatSoda.Widgets;

public abstract record InheritedWidget : Widget
{
    public required Widget Child { get; init; }

    /// <summary>
    /// 祖先マップへの登録キー。既定は具象型（Flutter の ExactType セマンティクス）。
    /// 基底型で lookup させたい階層（例: <see cref="WindowWidget"/>）はこれをオーバーライドする。
    /// </summary>
    public virtual Type ScopeType => GetType();

    public override Element CreateElement() => new InheritedElement() { Widget = this };

    public abstract bool UpdateShouldNotify(InheritedWidget oldWidget);
}