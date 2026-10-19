using FloatSoda.Abstractions.Geometries;
using FloatSoda.Rendering.Layers;
using FloatSoda.Geometrics;
using FloatSoda.Gesture;
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

    /// <summary><see cref="Clipper"/>が生成したクリップ領域。<see cref="MarkNeedsClip"/>まで再利用します。</summary>
    private T? _clipperClip;

    /// <summary><see cref="_clipperClip"/>が現在のクリッパーとサイズに対して有効かどうか。</summary>
    private bool _hasClipperClip;

    /// <summary>現在のサイズに対して有効なクリップ領域を取得します。</summary>
    /// <value><see cref="Clipper"/>が設定されている場合はその生成結果、それ以外は<see cref="DefaultClip"/>。</value>
    /// <remarks>
    /// <see cref="Clipper"/>の生成結果は、<see cref="MarkNeedsClip"/>が呼ばれるまで描画とヒットテストで共有します。
    /// ポインターが動くたびのヒットテストで<see cref="CustomClipper{T}.GetClip"/>を呼び直さないためです。
    /// </remarks>
    protected T Clip
    {
        get
        {
            if (Clipper == null) return DefaultClip;

            if (!_hasClipperClip)
            {
                _clipperClip = Clipper.GetClip(Size);
                _hasClipperClip = true;
            }

            return _clipperClip!;
        }
    }

    /// <summary><see cref="Clipper"/>が未設定のときに使用する既定のクリップ領域を取得します。</summary>
    /// <value>このRenderObjectの現在のサイズ全体を覆う領域。</value>
    protected abstract T DefaultClip { get; }

    /// <summary>このRenderObjectをPaint Dirtyとしてマークし、次のパイプライン更新時にクリップ領域を再計算します。</summary>
    protected void MarkNeedsClip()
    {
        _hasClipperClip = false;
        MarkNeedsPaint();
    }

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
    /// <remarks>
    /// <see cref="RenderCustomClip{T}.Clipper"/>が設定されている場合、クリップ領域の外の位置はヒットしません。
    /// 設定されていない場合のクリップ領域は自身の範囲と同じなので、通常の判定と変わりません。
    /// </remarks>
    public override bool HitTest(HitTestResult result, Offset position)
    {
        if (Clipper != null && !Clip.Contains((float)position.X, (float)position.Y)) return false;

        return base.HitTest(result, position);
    }

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
    /// <remarks>
    /// <see cref="RenderCustomClip{T}.Clipper"/>が設定されている場合、角丸矩形の外の位置はヒットしません。
    /// 設定されていない場合は角の外も含めて自身の範囲でヒットします。Flutter の <c>RenderClipRRect</c> に合わせています。
    /// </remarks>
    public override bool HitTest(HitTestResult result, Offset position)
    {
        if (Clipper != null && !Contains(Clip, (float)position.X, (float)position.Y)) return false;

        return base.HitTest(result, position);
    }

    /// <summary>位置が角丸矩形の内側にあるかを判定します。</summary>
    /// <param name="roundRect">判定する角丸矩形。</param>
    /// <param name="x">判定する位置のX座標。</param>
    /// <param name="y">判定する位置のY座標。</param>
    /// <returns>内側にある場合は<see langword="true"/>、それ以外の場合は<see langword="false"/>。</returns>
    /// <remarks>ポインターが動くたびに呼ばれるため、ネイティブのパスを作らずに角ごとの楕円で判定します。</remarks>
    private static bool Contains(SKRoundRect roundRect, float x, float y)
    {
        var rect = roundRect.Rect;
        if (!rect.Contains(x, y)) return false;

        var topLeft = roundRect.GetRadii(SKRoundRectCorner.UpperLeft);
        var topRight = roundRect.GetRadii(SKRoundRectCorner.UpperRight);
        var bottomRight = roundRect.GetRadii(SKRoundRectCorner.LowerRight);
        var bottomLeft = roundRect.GetRadii(SKRoundRectCorner.LowerLeft);

        if (x < rect.Left + topLeft.X && y < rect.Top + topLeft.Y)
            return InEllipse(x, y, rect.Left + topLeft.X, rect.Top + topLeft.Y, topLeft);
        if (x > rect.Right - topRight.X && y < rect.Top + topRight.Y)
            return InEllipse(x, y, rect.Right - topRight.X, rect.Top + topRight.Y, topRight);
        if (x > rect.Right - bottomRight.X && y > rect.Bottom - bottomRight.Y)
            return InEllipse(x, y, rect.Right - bottomRight.X, rect.Bottom - bottomRight.Y, bottomRight);
        if (x < rect.Left + bottomLeft.X && y > rect.Bottom - bottomLeft.Y)
            return InEllipse(x, y, rect.Left + bottomLeft.X, rect.Bottom - bottomLeft.Y, bottomLeft);

        return true;

        static bool InEllipse(float x, float y, float centerX, float centerY, SKPoint radii)
        {
            var dx = (x - centerX) / radii.X;
            var dy = (y - centerY) / radii.Y;
            return (dx * dx) + (dy * dy) <= 1;
        }
    }

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
    /// <remarks>
    /// <see cref="RenderCustomClip{T}.Clipper"/>が設定されている場合、パスの外の位置はヒットしません。
    /// 設定されていない場合のクリップ領域は自身の範囲と同じなので、通常の判定と変わりません。
    /// </remarks>
    public override bool HitTest(HitTestResult result, Offset position)
    {
        if (Clipper != null && !Clip.Contains((float)position.X, (float)position.Y)) return false;

        return base.HitTest(result, position);
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
    /// <remarks>
    /// <see cref="RenderCustomClip{T}.Clipper"/>の有無にかかわらず、クリップ領域に内接する楕円の外の位置はヒットしません。
    /// Flutter の <c>RenderClipOval</c> に合わせています。
    /// </remarks>
    public override bool HitTest(HitTestResult result, Offset position)
    {
        var clip = Clip;
        var dx = (position.X - clip.MidX) / clip.Width;
        var dy = (position.Y - clip.MidY) / clip.Height;
        if ((dx * dx) + (dy * dy) > 0.25) return false;

        return base.HitTest(result, position);
    }

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
