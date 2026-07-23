using FloatSoda.Rendering.Layers;
using FloatSoda.Geometrics;
using FloatSoda.RenderObjects.Painting;
using SkiaSharp;

namespace FloatSoda.Widgets.Paint;

/// <summary>
/// 子要素の描画を楕円の内側へ制限します。
/// </summary>
/// <seealso cref="RenderClipOval"/>
public record ClipOval : SingleChildRenderObjectWidget<RenderClipOval>
{
    /// <summary>
    /// 楕円の外接矩形を算出するクリッパーを取得します。
    /// <see langword="null"/>の場合は、このウィジェットの領域全体を外接矩形として使用します。
    /// </summary>
    public CustomClipper<SKRect>? Clipper { get; init; } = null;
    /// <summary>
    /// クリップ境界の描画方式を取得します。
    /// </summary>
    public Clip ClipBehavior { get; init; } = Clip.Antialias;

    /// <summary>
    /// このウィジェットの楕円クリップを適用するRenderObjectを生成します。
    /// </summary>
    /// <returns>クリッパーと描画方式を保持する新しいRenderObject。</returns>
    public override RenderClipOval CreateRenderObject()
    {
        return new RenderClipOval
        {
            Clipper = Clipper,
            ClipBehavior = ClipBehavior
        };
    }

    /// <summary>
    /// クリッパーと描画方式を既存のRenderObjectへ反映します。
    /// </summary>
    /// <param name="renderObject">このウィジェットに対応するRenderObject。</param>
    /// <remarks>
    /// クリップ領域の再計算が必要な変更、または描画方式の変更があった場合、
    /// 対象をPaint Dirtyとしてマークし、次のパイプライン更新時に再描画します。
    /// 再クリップが不要で描画方式も変更されなかった場合、Dirty状態は変更されません。
    /// </remarks>
    public override void UpdateRenderObject(RenderClipOval renderObject)
    {
        renderObject.Clipper = Clipper;
        renderObject.ClipBehavior = ClipBehavior;
    }
}

/// <summary>
/// 子要素の描画を矩形の内側へ制限します。
/// </summary>
/// <seealso cref="RenderClipRect"/>
public record ClipRect : SingleChildRenderObjectWidget<RenderClipRect>
{
    /// <summary>
    /// クリップ矩形を算出するクリッパーを取得します。
    /// <see langword="null"/>の場合は、このウィジェットの領域全体を使用します。
    /// </summary>
    public CustomClipper<SKRect>? Clipper { get; init; } = null;
    /// <summary>
    /// クリップ境界の描画方式を取得します。
    /// </summary>
    public Clip ClipBehavior { get; init; } = Clip.Antialias;


    /// <summary>
    /// このウィジェットの矩形クリップを適用するRenderObjectを生成します。
    /// </summary>
    /// <returns>クリッパーと描画方式を保持する新しいRenderObject。</returns>
    public override RenderClipRect CreateRenderObject()
    {
        return new RenderClipRect
        {
            Clipper = Clipper,
            ClipBehavior = ClipBehavior
        };
    }

    /// <summary>
    /// クリッパーと描画方式を既存のRenderObjectへ反映します。
    /// </summary>
    /// <param name="renderObject">このウィジェットに対応するRenderObject。</param>
    /// <remarks>
    /// クリップ領域の再計算が必要な変更、または描画方式の変更があった場合、
    /// 対象をPaint Dirtyとしてマークし、次のパイプライン更新時に再描画します。
    /// 再クリップが不要で描画方式も変更されなかった場合、Dirty状態は変更されません。
    /// </remarks>
    public override void UpdateRenderObject(RenderClipRect renderObject)
    {
        renderObject.Clipper = Clipper;
        renderObject.ClipBehavior = ClipBehavior;
    }
}

/// <summary>
/// 子要素の描画を角丸矩形の内側へ制限します。
/// </summary>
/// <seealso cref="RenderClipRoundRect"/>
public record ClipRoundRect : SingleChildRenderObjectWidget<RenderClipRoundRect>
{
    /// <summary>
    /// 既定のクリップ領域へ適用する角の半径を取得または設定します。
    /// </summary>
    public BorderRadius BorderRadius;
    /// <summary>
    /// 角丸クリップ矩形を算出するクリッパーを取得します。
    /// <see langword="null"/>の場合は、<see cref="BorderRadius"/>を領域全体へ適用します。
    /// </summary>
    public CustomClipper<SKRoundRect>? Clipper { get; init; } = null;
    /// <summary>
    /// クリップ境界の描画方式を取得します。
    /// </summary>
    public Clip ClipBehavior { get; init; } = Clip.Antialias;


    /// <summary>
    /// このウィジェットの角丸矩形クリップを適用するRenderObjectを生成します。
    /// </summary>
    /// <returns>角の半径、クリッパー、および描画方式を保持する新しいRenderObject。</returns>
    public override RenderClipRoundRect CreateRenderObject()
    {
        return new RenderClipRoundRect
        {
            BorderRadius = BorderRadius,
            Clipper = Clipper,
            ClipBehavior = ClipBehavior
        };
    }

    /// <summary>
    /// 角の半径、クリッパー、および描画方式を既存のRenderObjectへ反映します。
    /// </summary>
    /// <param name="renderObject">このウィジェットに対応するRenderObject。</param>
    /// <remarks>
    /// クリップ領域の再計算が必要なクリッパーの変更、または描画方式の変更があった場合、
    /// 対象をPaint Dirtyとしてマークし、次のパイプライン更新時に再描画します。
    /// 角の半径だけが変更された場合、この更新ではDirty状態を変更しません。
    /// </remarks>
    public override void UpdateRenderObject(RenderClipRoundRect renderObject)
    {
        renderObject.BorderRadius = BorderRadius;
        renderObject.Clipper = Clipper;
        renderObject.ClipBehavior = ClipBehavior;
    }
}

/// <summary>
/// 子要素の描画を任意のパスの内側へ制限します。
/// </summary>
/// <seealso cref="RenderClipPath"/>
public record ClipCustomPath : SingleChildRenderObjectWidget<RenderClipPath>
{
    /// <summary>
    /// クリップパスを算出するクリッパーを取得します。
    /// <see langword="null"/>の場合は、このウィジェットの領域全体を囲む矩形パスを使用します。
    /// </summary>
    public CustomClipper<SKPath>? Clipper { get; init; } = null;
    /// <summary>
    /// クリップ境界の描画方式を取得します。
    /// </summary>
    public Clip ClipBehavior { get; init; } = Clip.Antialias;


    /// <summary>
    /// このウィジェットのパスクリップを適用するRenderObjectを生成します。
    /// </summary>
    /// <returns>クリッパーと描画方式を保持する新しいRenderObject。</returns>
    public override RenderClipPath CreateRenderObject()
    {
        return new RenderClipPath()
        {
            Clipper = Clipper,
            ClipBehavior = ClipBehavior
        };
    }

    /// <summary>
    /// クリッパーと描画方式を既存のRenderObjectへ反映します。
    /// </summary>
    /// <param name="renderObject">このウィジェットに対応するRenderObject。</param>
    /// <remarks>
    /// クリップ領域の再計算が必要な変更、または描画方式の変更があった場合、
    /// 対象をPaint Dirtyとしてマークし、次のパイプライン更新時に再描画します。
    /// 再クリップが不要で描画方式も変更されなかった場合、Dirty状態は変更されません。
    /// </remarks>
    public override void UpdateRenderObject(RenderClipPath renderObject)
    {
        renderObject.Clipper = Clipper;
        renderObject.ClipBehavior = ClipBehavior;
    }
}