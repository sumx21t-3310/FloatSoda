using FloatSoda.Geometrics;
using FloatSoda.RenderObjects.Layout;

namespace FloatSoda.Widgets.Layout;

/// <summary>
/// 子要素の周囲に内側の余白を追加します。
/// </summary>
/// <seealso cref="RenderPadding"/>
public record Padding : SingleChildRenderObjectWidget<RenderPadding>
{
    /// <summary>
    /// 子要素の周囲に適用する余白を取得します。
    /// </summary>
    public required EdgeInsets Spacing { get; init; }

    /// <summary>
    /// このウィジェットの余白を適用するRenderObjectを生成します。
    /// </summary>
    /// <returns>指定された余白を保持する新しいRenderObject。</returns>
    public override RenderPadding CreateRenderObject() => new()
    {
        Padding = Spacing
    };

    /// <summary>
    /// 余白を既存のRenderObjectへ反映します。
    /// </summary>
    /// <param name="renderObject">このウィジェットに対応するRenderObject。</param>
    /// <remarks>
    /// 余白が変更された場合、対象をLayout Dirtyとしてマークし、
    /// 次のパイプライン更新時に子要素の制約、位置、および自身のサイズを再計算します。
    /// 値が変更されなかった場合、Dirty状態は変更されません。
    /// </remarks>
    public override void UpdateRenderObject(RenderPadding renderObject)
    {
        renderObject.Padding = Spacing;
    }
}
