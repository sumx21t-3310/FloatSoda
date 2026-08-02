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
    /// 親から受け取った制約へ追加し、子要素のレイアウト時に適用する寸法制約を取得します。
    /// </summary>
    public required BoxConstraints AdditionalConstraints { get; init; }

    /// <summary>
    /// このウィジェットの追加制約を保持するRenderObjectを生成します。
    /// </summary>
    /// <returns>指定された追加制約を保持する新しいRenderObject。</returns>
    public override RenderConstrainedBox CreateRenderObject() => new()
    {
        AdditionalConstraints = AdditionalConstraints
    };

    /// <summary>
    /// 追加制約を既存のRenderObjectへ反映します。
    /// </summary>
    /// <param name="renderObject">このウィジェットに対応するRenderObject。</param>
    /// <remarks>
    /// 制約が変更された場合、対象をLayout Dirtyとしてマークし、
    /// 次のパイプライン更新時に子要素のサイズを再計算します。
    /// 制約が変更されなかった場合、Dirty状態は変更されません。
    /// </remarks>
    public override void UpdateRenderObject(RenderConstrainedBox renderObject)
        => renderObject.AdditionalConstraints = AdditionalConstraints;
}
