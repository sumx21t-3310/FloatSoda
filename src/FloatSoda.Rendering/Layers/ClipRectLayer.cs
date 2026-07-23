using SkiaSharp;

namespace FloatSoda.Rendering.Layers;

/// <summary>
/// すべての子レイヤーの描画を矩形の内側へ制限するレイヤーです。
/// </summary>
/// <param name="clipRect">
/// 子レイヤーの描画を許可する矩形。
/// </param>
/// <seealso cref="ClipRoundRectLayer"/>
/// <seealso cref="ClipPathLayer"/>
public class ClipRectLayer(SKRect clipRect) : ContainerLayer
{
    /// <summary>
    /// 子レイヤーの描画を許可する矩形を取得または設定します。
    /// </summary>
    public SKRect ClipRect { get; set; } = clipRect;
    /// <summary>
    /// クリップ境界の描画品質を取得または設定します。
    /// </summary>
    /// <value>
    /// 境界に適用するクリップ方式。既定値は<see cref="Clip.Antialias"/>です。
    /// </value>
    public Clip ClipBehavior { get; set; } = Clip.Antialias;


    /// <summary>
    /// 子レイヤーをレイアウトし、クリップ矩形と重なる描画領域を評価します。
    /// </summary>
    /// <param name="context">
    /// 子レイヤーのレイアウトに使用するコンテキスト。
    /// </param>
    public override void Layout(LayerContext context)
    {
        var clipPathBounds = LayoutChildren(context);

        if (!SKRect.Intersect(ClipRect, clipPathBounds).IsEmpty)
        {
            PaintBounds = clipPathBounds;
        }

        context.Canvas.Restore();
    }

    /// <summary>
    /// クリップ矩形の内側に限って子レイヤーを描画します。
    /// </summary>
    /// <param name="context">
    /// クリップした子レイヤーを合成するキャンバスを保持するコンテキスト。
    /// </param>
    /// <remarks>
    /// 描画後にキャンバスの状態を復元するため、後続のレイヤーへクリップは引き継がれません。
    /// </remarks>
    public override void Paint(LayerContext context)
    {
        context.Canvas.Save();
        context.Canvas.ClipRect(ClipRect, antialias: ClipBehavior == Clip.Antialias);

        base.Paint(context);
        context.Canvas.Restore();
    }

    /// <summary>
    /// クリップ矩形、クリップ方式、および子レイヤーを保持する新しいレイヤーを作成します。
    /// </summary>
    /// <returns>
    /// 子レイヤーを再帰的に複製した新しい矩形クリップレイヤー。
    /// </returns>
    public override ILayer Clone()
    {
        var cloned = new ClipRectLayer(ClipRect)
        {
            ClipBehavior = ClipBehavior
        };

        foreach (var child in Children)
        {
            cloned.Children.Add(child.Clone());
        }

        return cloned;
    }
}

/// <summary>
/// すべての子レイヤーの描画を角丸矩形の内側へ制限するレイヤーです。
/// </summary>
/// <param name="clipRect">
/// 子レイヤーの描画を許可する角丸矩形。所有権は呼び出し元に残ります。
/// </param>
/// <seealso cref="ClipRectLayer"/>
/// <seealso cref="ClipPathLayer"/>
public class ClipRoundRectLayer(SKRoundRect clipRect) : ContainerLayer
{
    /// <summary>
    /// 子レイヤーの描画を許可する角丸矩形を取得または設定します。
    /// </summary>
    /// <value>
    /// クリップに使用する角丸矩形。所有権は呼び出し元に残ります。
    /// </value>
    public SKRoundRect ClipRect { get; set; } = clipRect;
    /// <summary>
    /// クリップ境界の描画品質を取得または設定します。
    /// </summary>
    /// <value>
    /// 境界に適用するクリップ方式。既定値は<see cref="Clip.Antialias"/>です。
    /// </value>
    public Clip ClipBehavior { get; set; } = Clip.Antialias;


    /// <summary>
    /// 子レイヤーをレイアウトし、角丸矩形の外接矩形と重なる描画領域を評価します。
    /// </summary>
    /// <param name="context">
    /// 子レイヤーのレイアウトに使用するコンテキスト。
    /// </param>
    public override void Layout(LayerContext context)
    {
        var clipPathBounds = LayoutChildren(context);

        if (!SKRect.Intersect(ClipRect.Rect, clipPathBounds).IsEmpty)
        {
            PaintBounds = clipPathBounds;
        }

        context.Canvas.Restore();
    }

    /// <summary>
    /// 角丸矩形の内側に限って子レイヤーを描画します。
    /// </summary>
    /// <param name="context">
    /// クリップした子レイヤーを合成するキャンバスを保持するコンテキスト。
    /// </param>
    /// <remarks>
    /// 描画後にキャンバスの状態を復元するため、後続のレイヤーへクリップは引き継がれません。
    /// </remarks>
    public override void Paint(LayerContext context)
    {
        context.Canvas.Save();
        context.Canvas.ClipRoundRect(ClipRect, antialias: ClipBehavior == Clip.Antialias);

        base.Paint(context);
        context.Canvas.Restore();
    }

    /// <summary>
    /// 角丸矩形、クリップ方式、および子レイヤーを保持する新しいレイヤーを作成します。
    /// </summary>
    /// <returns>
    /// 子レイヤーを再帰的に複製した新しい角丸矩形クリップレイヤー。
    /// </returns>
    public override ILayer Clone()
    {
        var cloned = new ClipRoundRectLayer(ClipRect)
        {
            ClipBehavior = ClipBehavior
        };

        foreach (var child in Children)
        {
            cloned.Children.Add(child.Clone());
        }

        return cloned;
    }
}

/// <summary>
/// すべての子レイヤーの描画をパスの内側へ制限するレイヤーです。
/// </summary>
/// <param name="clipPath">
/// 子レイヤーの描画を許可するパス。所有権は呼び出し元に残ります。
/// </param>
/// <seealso cref="ClipRectLayer"/>
/// <seealso cref="ClipRoundRectLayer"/>
public class ClipPathLayer(SKPath clipPath) : ContainerLayer
{
    /// <summary>
    /// 子レイヤーの描画を許可するパスを取得または設定します。
    /// </summary>
    /// <value>
    /// クリップに使用するパス。所有権は呼び出し元に残り、使用中は有効な状態に保つ必要があります。
    /// </value>
    public SKPath ClipPath { get; set; } = clipPath;
    /// <summary>
    /// クリップ境界の描画品質を取得または設定します。
    /// </summary>
    /// <value>
    /// 境界に適用するクリップ方式。既定値は<see cref="Clip.Antialias"/>です。
    /// </value>
    public Clip ClipBehavior { get; set; } = Clip.Antialias;

    /// <summary>
    /// 子レイヤーをレイアウトし、クリップパスの境界と重なる描画領域を評価します。
    /// </summary>
    /// <param name="context">
    /// 子レイヤーのレイアウトに使用するコンテキスト。
    /// </param>
    public override void Layout(LayerContext context)
    {
        var clipPathBounds = ClipPath.Bounds;
        var clipPaintBounds = LayoutChildren(context);

        if (!SKRect.Intersect(clipPathBounds, clipPaintBounds).IsEmpty)
        {
            PaintBounds = clipPathBounds;
        }
    }

    /// <summary>
    /// クリップパスの内側に限って子レイヤーを描画します。
    /// </summary>
    /// <param name="context">
    /// クリップした子レイヤーを合成するキャンバスを保持するコンテキスト。
    /// </param>
    /// <remarks>
    /// 描画後にキャンバスの状態を復元するため、後続のレイヤーへクリップは引き継がれません。
    /// </remarks>
    public override void Paint(LayerContext context)
    {
        context.Canvas.Save();
        context.Canvas.ClipPath(ClipPath, antialias: ClipBehavior == Clip.Antialias);
        base.Paint(context);
        context.Canvas.Restore();
    }

    /// <summary>
    /// クリップパス、クリップ方式、および子レイヤーを保持する新しいレイヤーを作成します。
    /// </summary>
    /// <returns>
    /// 子レイヤーを再帰的に複製した新しいパスクリップレイヤー。
    /// </returns>
    /// <remarks>
    /// クリップパスの所有権は移転せず、複製元と同じパスを参照します。
    /// </remarks>
    public override ILayer Clone()
    {
        var cloned = new ClipPathLayer(ClipPath) { ClipBehavior = ClipBehavior };

        foreach (var child in Children)
        {
            cloned.Children.Add(child.Clone());
        }

        return cloned;
    }
}

/// <summary>
/// クリップ境界の描画方式を指定します。
/// </summary>
public enum Clip
{
    /// <summary>
    /// 境界へアンチエイリアスを適用しません。
    /// </summary>
    None,
    /// <summary>
    /// 境界をアンチエイリアスなしの明瞭な輪郭で描画します。
    /// </summary>
    HardEdge,
    /// <summary>
    /// 境界へアンチエイリアスを適用して描画します。
    /// </summary>
    Antialias
}