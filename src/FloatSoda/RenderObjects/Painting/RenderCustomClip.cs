using FloatSoda.Abstractions.Geometries;
using FloatSoda.Rendering.Layers;
using FloatSoda.Geometrics;
using SkiaSharp;

namespace FloatSoda.RenderObjects.Painting;

/// <summary>
/// RenderObjectのサイズから任意のクリップ領域を生成する基底クラスです。
/// </summary>
/// <typeparam name="T">生成するクリップ領域の型。</typeparam>
public abstract class CustomClipper<T>
{
    /// <summary>
    /// 指定したサイズに対応するクリップ領域を生成します。
    /// </summary>
    /// <param name="size">クリップ対象となるRenderObjectのサイズ。</param>
    /// <returns>生成されたクリップ領域。</returns>
    public abstract T GetClip(SKSize size);

    /// <summary>
    /// 以前のクリッパーからクリップ領域を再生成する必要があるかを判定します。
    /// </summary>
    /// <param name="oldClipper">以前使用していたクリッパー。</param>
    /// <returns>クリップ領域を再生成する必要がある場合は<c>true</c>、再利用できる場合は<c>false</c>。</returns>
    public abstract bool ShouldReclip(CustomClipper<T> oldClipper);
}

/// <summary>
/// 子の描画を任意の形状で切り抜くRenderObjectの基底クラスです。
/// </summary>
/// <typeparam name="T">クリップ領域を表す型。</typeparam>
public abstract class RenderCustomClip<T> : RenderProxyBox
{
    /// <summary>
    /// クリップ領域を生成するクリッパーを取得または設定します。
    /// </summary>
    /// <remarks>
    /// クリッパーが追加または削除された場合、型が変わった場合、または新しいクリッパーが
    /// 再生成を要求した場合、このRenderObjectをPaint Dirtyとしてマークし、
    /// 次のパイプライン更新時にクリップ領域と子を再描画します。
    /// 同じクリッパーが設定された場合、またはクリップ領域を再利用できる場合、Dirty状態は変更されません。
    /// </remarks>
    public CustomClipper<T>? Clipper
    {
        get;
        set
        {
            if (field == value) return;

            var oldClipper = field;
            field = value;

            if (value == null || oldClipper == null || value.GetType() != oldClipper.GetType() ||
                value.ShouldReclip(oldClipper))
            {
                MarkNeedsClip();
            }
        }
    }


    /// <summary>
    /// クリップ境界に適用する描画品質を取得または設定します。
    /// </summary>
    /// <remarks>
    /// 値が変更された場合、このRenderObjectをPaint Dirtyとしてマークし、
    /// 次のパイプライン更新時に指定した品質で子を再描画します。
    /// 値が変更されなかった場合、Dirty状態は変更されません。
    /// </remarks>
    public Clip ClipBehavior
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            MarkNeedsPaint();
        }
    } = FloatSoda.Rendering.Layers.Clip.Antialias;

    /// <summary>現在のサイズに対して有効なクリップ領域を取得します。</summary>
    /// <value><see cref="Clipper"/>が設定されている場合はその生成結果、それ以外は<see cref="DefaultClip"/>。</value>
    protected T Clip => Clipper != null ? Clipper.GetClip(Size) : DefaultClip;

    /// <summary><see cref="Clipper"/>が未設定のときに使用する既定のクリップ領域を取得します。</summary>
    /// <value>このRenderObjectの現在のサイズ全体を覆う領域。</value>
    protected abstract T DefaultClip { get; }

    /// <summary>このRenderObjectをPaint Dirtyとしてマークし、次のパイプライン更新時にクリップ領域を再計算します。</summary>
    protected void MarkNeedsClip() => MarkNeedsPaint();

    /// <inheritdoc/>
    public override void PerformLayout()
    {
        var oldSize = Size;

        base.PerformLayout();

        if (oldSize != Size) MarkNeedsClip();
    }
}

/// <summary>
/// 子の描画を矩形領域で切り抜くRenderObjectです。
/// </summary>
public class RenderClipRect : RenderCustomClip<SKRect>
{
    /// <inheritdoc/>
    protected override SKRect DefaultClip => SKRect.Create(Offset.Zero, Size);

    /// <inheritdoc/>
    public override void Paint(PaintingContext context, Offset offset)
    {
        if (Child != null)
        {
            Layer = context.PushClipRect(
                offset,
                Clip,
                (c, o) => base.Paint(c, o), ClipBehavior,
                Layer as ClipRectLayer);
        }
        else
        {
            Layer = null;
        }
    }
}

/// <summary>
/// 子の描画を角丸矩形領域で切り抜くRenderObjectです。
/// </summary>
public class RenderClipRoundRect : RenderCustomClip<SKRoundRect>
{
    /// <summary>
    /// 既定のクリップ領域に適用する角の半径を取得または設定します。
    /// </summary>
    /// <remarks>
    /// このプロパティの設定だけではDirty状態は変更されません。
    /// 変更後に再描画が要求されたとき、新しい角の半径がクリップ領域へ反映されます。
    /// </remarks>
    public BorderRadius BorderRadius { get; set; }

    /// <inheritdoc/>
    protected override SKRoundRect DefaultClip => BorderRadius.ToRoundRect(SKRect.Create(Offset.Zero, Size));

    /// <inheritdoc/>
    public override void Paint(PaintingContext context, Offset offset)
    {
        if (Child != null)
        {
            Layer = context.PushClipRoundRect(
                offset,
                Clip.Rect,
                Clip,
                (c, o) => base.Paint(c, o),
                ClipBehavior,
                Layer as ClipRoundRectLayer
            );
        }
        else
        {
            Layer = null;
        }
    }
}

/// <summary>
/// 子の描画をパス領域で切り抜くRenderObjectです。
/// </summary>
public class RenderClipPath : RenderCustomClip<SKPath>
{
    /// <inheritdoc/>
    protected override SKPath DefaultClip
    {
        get
        {
            var path = new SKPath();
            path.AddRect(SKRect.Create(Offset.Zero, Size));
            return path;
        }
    }

    /// <inheritdoc/>
    public override void Paint(PaintingContext context, Offset offset)
    {
        if (Child != null)
        {
            Layer = context.PushClipPath(
                offset,
                SKRect.Create(Offset.Zero, Size),
                Clip,
                (c, o) => base.Paint(c, o),
                ClipBehavior,
                Layer as ClipPathLayer
            );
        }
        else
        {
            Layer = null;
        }
    }
}

/// <summary>
/// 子の描画を楕円領域で切り抜くRenderObjectです。
/// </summary>
public class RenderClipOval : RenderCustomClip<SKRect>
{
    /// <inheritdoc/>
    protected override SKRect DefaultClip => SKRect.Create(Offset.Zero, Size);

    /// <inheritdoc/>
    public override void Paint(PaintingContext context, Offset offset)
    {
        if (Child == null) return;

        var path = new SKPath();
        path.AddOval(Clip);

        Layer = context.PushClipPath(
            offset,
            Clip,
            path,
            (c, o) => base.Paint(c, o),
            ClipBehavior,
            Layer as ClipPathLayer
        );
    }
}
