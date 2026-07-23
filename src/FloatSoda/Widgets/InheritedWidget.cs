using FloatSoda.Elements;

namespace FloatSoda.Widgets;

/// <summary>
/// 子孫Elementへ共有値を提供し、その値への依存関係を追跡するウィジェットの基底型です。
/// </summary>
/// <seealso cref="InheritedElement"/>
public abstract record InheritedWidget : Widget
{
    /// <summary>
    /// この共有スコープの直下に配置する子ウィジェットを取得します。
    /// </summary>
    public required Widget Child { get; init; }

    /// <summary>
    /// 祖先マップへの登録キー。既定は具象型（Flutter の ExactType セマンティクス）。
    /// 基底型で lookup させたい階層（例: <see cref="WindowWidget"/>）はこれをオーバーライドする。
    /// </summary>
    public virtual Type ScopeType => GetType();

    /// <summary>
    /// このウィジェットを祖先マップへ登録し、依存する子孫を管理するElementを生成します。
    /// </summary>
    /// <returns>このウィジェットを保持する未マウントのElement。</returns>
    public override Element CreateElement() => new InheritedElement() { Widget = this };

    /// <summary>
    /// 更新前後の共有値を比較し、依存する子孫Elementへ変更を通知するかを判定します。
    /// </summary>
    /// <param name="oldWidget">更新前に同じElementが保持していたウィジェット。</param>
    /// <returns>依存する子孫へ変更を通知する場合は<see langword="true"/>。通知しない場合は<see langword="false"/>。</returns>
    /// <remarks>
    /// <see langword="true"/>を返すと、登録済みの依存Elementへ再構築を要求します。
    /// </remarks>
    public abstract bool UpdateShouldNotify(InheritedWidget oldWidget);
}