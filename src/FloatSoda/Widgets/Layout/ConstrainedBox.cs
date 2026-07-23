using FloatSoda.Geometrics;
using FloatSoda.RenderObjects.Layout;

namespace FloatSoda.Widgets.Layout;

/// <summary>
/// 親から受け取った制約へ追加の寸法制約を適用して子要素をレイアウトします。
/// </summary>
/// <seealso cref="RenderConstrainedBox"/>
public record ConstrainedBox : SingleChildRenderObjectWidget<RenderConstrainedBox>
{
    /// <summary>
    /// 子要素のレイアウト時に追加する寸法制約を取得します。
    /// </summary>
    public BoxConstraints  Constraints { get; init; }
    /// <summary>
    /// このウィジェットの追加制約を保持するRenderObjectを生成します。
    /// </summary>
    /// <returns>指定された追加制約を保持する新しいRenderObject。</returns>
    public override RenderConstrainedBox CreateRenderObject() => new RenderConstrainedBox
    {
        AdditionalConstraints = Constraints
    };
}