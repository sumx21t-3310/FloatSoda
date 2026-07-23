using SkiaSharp;

namespace FloatSoda.Rendering.Layers;

/// <summary>
/// 子レイヤーを保持し、一覧の順序で同じ描画先へ合成するレイヤーです。
/// </summary>
/// <seealso cref="PictureLayer"/>
public class ContainerLayer : ILayer
{
    /// <summary>
    /// 合成する子レイヤーの一覧を取得します。
    /// </summary>
    /// <value>
    /// 前の要素から順に描画される、変更可能な子レイヤーの一覧。
    /// </value>
    public List<ILayer> Children { get; } = [];

    /// <summary>
    /// 子レイヤーが1つ以上存在するかどうかを取得します。
    /// </summary>
    /// <value>
    /// 子レイヤーが存在する場合は<see langword="true"/>、存在しない場合は<see langword="false"/>。
    /// </value>
    public bool HasChildren => Children.Count != 0;

    /// <summary>
    /// レイアウト後のすべての子レイヤーを包含する描画境界を取得します。
    /// </summary>
    /// <value>
    /// 親レイヤーの座標系で表した子レイヤーの描画境界の和集合。
    /// </value>
    public SKRect PaintBounds { get; protected set; }

    /// <summary>
    /// すべての子レイヤーをレイアウトし、それらを包含する描画境界を設定します。
    /// </summary>
    /// <param name="context">
    /// 子レイヤーのレイアウトに使用するコンテキスト。
    /// </param>
    public virtual void Layout(LayerContext context) => PaintBounds = LayoutChildren(context);

    /// <summary>
    /// すべての子レイヤーをレイアウトし、それらを包含する描画境界を返します。
    /// </summary>
    /// <param name="context">
    /// 子レイヤーのレイアウトに使用するコンテキスト。
    /// </param>
    /// <returns>
    /// 子レイヤーが描画へ影響する境界の和集合。子が存在しない場合は空の矩形。
    /// </returns>
    protected SKRect LayoutChildren(LayerContext context)
    {
        var bounds = SKRect.Empty;

        foreach (var child in Children)
        {
            child.Layout(context);
            bounds.Union(child.PaintBounds);
        }

        return bounds;
    }

    /// <summary>
    /// 子レイヤーを一覧の先頭から順に描画します。
    /// </summary>
    /// <param name="context">
    /// 子レイヤーの合成先となるキャンバスを保持するコンテキスト。
    /// </param>
    public virtual void Paint(LayerContext context)
    {
        foreach (var child in Children)
        {
            child.Paint(context);
        }
    }

    /// <summary>
    /// すべての子レイヤーを再帰的に複製した、新しいコンテナーレイヤーを作成します。
    /// </summary>
    /// <returns>
    /// 子レイヤーの一覧を独立して保持する新しいコンテナーレイヤー。
    /// </returns>
    public virtual ILayer Clone()
    {
        var cloned = new ContainerLayer();

        foreach (var child in Children)
        {
            cloned.Children.Add(child.Clone());
        }

        return cloned;
    }
}