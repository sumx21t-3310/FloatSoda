using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.RenderObjects.Layout;

namespace FloatSoda.Widgets.Layout;

/// <summary>
/// 子要素を利用可能な領域内の指定位置へ配置します。
/// </summary>
/// <seealso cref="RenderPositionedBox"/>
public record Align : SingleChildRenderObjectWidget<RenderPositionedBox>
{
    /// <summary>
    /// 子要素を親の領域内へ配置する基準位置を取得します。
    /// </summary>
    public Alignment Alignment { get; init; } = Alignment.Center;
    /// <summary>
    /// 子要素の幅に乗算して自身の幅を決める係数を取得します。
    /// <see langword="null"/>の場合は、制約で許可された最大幅を使用します。
    /// </summary>
    public double? WidthFactor { get; init; } = null;
    /// <summary>
    /// 子要素の高さに乗算して自身の高さを決める係数を取得します。
    /// <see langword="null"/>の場合は、制約で許可された最大高さを使用します。
    /// </summary>
    public double? HeightFactor { get; init; } = null;


    /// <summary>
    /// このウィジェットの配置設定を保持するRenderObjectを生成します。
    /// </summary>
    /// <returns>配置基準と寸法係数を保持する新しいRenderObject。</returns>
    public override RenderPositionedBox CreateRenderObject()
    {
        return new RenderPositionedBox
        {
            Alignment = Alignment,
            WidthFactor = WidthFactor,
            HeightFactor = HeightFactor
        };
    }

    /// <summary>
    /// 配置基準と寸法係数を既存のRenderObjectへ反映します。
    /// </summary>
    /// <param name="renderObject">このウィジェットに対応するRenderObject。</param>
    /// <remarks>
    /// いずれかの値が変更された場合、対象をLayout Dirtyとしてマークし、
    /// 次のパイプライン更新時に子要素のサイズと位置を再計算します。
    /// すべての値が変更されなかった場合、Dirty状態は変更されません。
    /// </remarks>
    public override void UpdateRenderObject(RenderPositionedBox renderObject)
    {
        renderObject.WidthFactor = WidthFactor;
        renderObject.HeightFactor = HeightFactor;
        renderObject.Alignment = Alignment;
    }
}

/// <summary>
/// 子要素を利用可能な領域の中央に配置します。
/// </summary>
/// <seealso cref="Align"/>
/// <seealso cref="RenderPositionedBox"/>
public record Center : StatelessWidget
{
    /// <summary>
    /// 中央へ配置する子ウィジェットを取得します。
    /// <see langword="null"/>の場合は子要素を配置しません。
    /// </summary>
    public Widget? Child { get; init; } = null;

    /// <summary>
    /// 子要素を中央へ配置するウィジェットを構築します。
    /// </summary>
    /// <param name="context">このウィジェットが配置されている構築コンテキスト。</param>
    /// <returns>中央配置を指定した<see cref="Align"/>。</returns>
    public override Widget Build(IBuildContext context)
    {
        return new Align()
        {
            Child = Child,
            Alignment = Alignment.Center
        };
    }
}