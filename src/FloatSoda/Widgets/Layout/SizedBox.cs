using FloatSoda.Geometrics;
using FloatSoda.RenderObjects.Layout;

namespace FloatSoda.Widgets.Layout;

/// <summary>
/// 指定した幅または高さの厳密な制約を子要素へ適用します。
/// </summary>
/// <seealso cref="RenderConstrainedBox"/>
public record SizedBox : SingleChildRenderObjectWidget<RenderConstrainedBox>
{
    /// <summary>
    /// 子要素へ要求する幅を論理ピクセル単位で取得します。
    /// <see langword="null"/>の場合は幅を固定しません。
    /// </summary>
    public double? Width { get; init; } = null;
    /// <summary>
    /// 子要素へ要求する高さを論理ピクセル単位で取得します。
    /// <see langword="null"/>の場合は高さを固定しません。
    /// </summary>
    public double? Height { get; init; } = null;


    /// <summary>
    /// 指定された幅と高さから厳密な制約を構成するRenderObjectを生成します。
    /// </summary>
    /// <returns>幅と高さの制約を保持する新しいRenderObject。</returns>
    public override RenderConstrainedBox CreateRenderObject()
    {
        return new RenderConstrainedBox
        {
            AdditionalConstraints = BoxConstraints.TightFor(Width, Height)
        };
    }

    /// <summary>
    /// 指定された幅と高さから構成した厳密な制約を既存のRenderObjectへ反映します。
    /// </summary>
    /// <param name="renderObject">このウィジェットに対応するRenderObject。</param>
    /// <remarks>
    /// 制約が変更された場合、対象をLayout Dirtyとしてマークし、
    /// 次のパイプライン更新時に子要素のサイズを再計算します。
    /// 制約が変更されなかった場合、Dirty状態は変更されません。
    /// </remarks>
    public override void UpdateRenderObject(RenderConstrainedBox renderObject) => renderObject.AdditionalConstraints = BoxConstraints.TightFor(Width, Height);
}