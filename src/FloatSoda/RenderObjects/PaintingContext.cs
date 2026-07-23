using FloatSoda.Abstractions.Geometries;
using FloatSoda.Rendering.Layers;
using SkiaSharp;

namespace FloatSoda.RenderObjects;

/// <summary>RenderObjectの描画命令を画像レイヤーと合成レイヤーへ記録します。</summary>
/// <param name="containerLayer">生成した子レイヤーを追加するコンテナレイヤー。</param>
/// <param name="estimatedBounds">画像の記録に使用する推定描画範囲。</param>
/// <remarks>一つのインスタンスは一つのコンテナレイヤーへ記録し、必要に応じて子用のコンテキストを生成します。</remarks>
public class PaintingContext(ContainerLayer containerLayer, SKRect estimatedBounds)
{
    /// <summary>現在記録中の画像レイヤーです。</summary>
    private PictureLayer? _currentLayer;

    /// <summary>現在の画像を構築する記録器です。</summary>
    private SKPictureRecorder? _recorder;

    /// <summary>現在の画像へ描画命令を記録するキャンバスです。</summary>
    private SKCanvas? _canvas;

    /// <summary>画像への描画命令を記録中かどうかを取得します。</summary>
    private bool IsRecording => _canvas != null;

    /// <summary>描画命令を記録するキャンバスを取得します。</summary>
    /// <value>このコンテキストの現在の画像へ記録するキャンバス。</value>
    /// <remarks>最初の取得時に画像レイヤーと記録器を生成し、対象のコンテナレイヤーへ画像レイヤーを追加します。</remarks>
    public SKCanvas Canvas
    {
        get
        {
            if (_canvas == null) StartRecording();
            return _canvas!;
        }
    }

    /// <summary>新しい画像レイヤーへの描画記録を開始します。</summary>
    /// <remarks>生成した画像レイヤーを対象のコンテナレイヤーへ直ちに追加します。</remarks>
    private void StartRecording()
    {
        _currentLayer = new PictureLayer();
        _recorder = new SKPictureRecorder();
        containerLayer.Children.Add(_currentLayer);
        _canvas = _recorder.BeginRecording(estimatedBounds);
    }

    /// <summary>描画記録中であれば画像を確定して記録を終了します。</summary>
    /// <remarks>記録していない場合は何もしません。確定した画像は現在の画像レイヤーへ設定されます。</remarks>
    public void StopRecordingIfNeeded()
    {
        if (!IsRecording) return;

        _currentLayer?.Picture = _recorder?.EndRecording();
        _currentLayer = null;
        _recorder = null;
        _canvas = null;
    }

    /// <summary>子のコンテナレイヤーへ描画内容を記録します。</summary>
    /// <param name="childLayer">描画内容を保持する子レイヤー。既存の子レイヤーは消去して再利用されます。</param>
    /// <param name="painter">子用の描画コンテキストへ描画命令を記録する処理。</param>
    /// <param name="offset">親の座標系における子の描画原点。</param>
    /// <param name="childPaintBounds">子の推定描画範囲。省略した場合はこのコンテキストの推定描画範囲を使用します。</param>
    /// <remarks>
    /// 現在の画像記録を確定してから子レイヤーを対象のコンテナへ追加します。
    /// 描画処理の完了後、子コンテキストに残る画像記録も確定します。
    /// </remarks>
    public void PushLayer(
        ContainerLayer childLayer,
        Action<PaintingContext, Offset> painter,
        Offset offset,
        SKRect? childPaintBounds = null)
    {
        if (childLayer.HasChildren)
        {
            childLayer.Children.Clear();
        }

        StopRecordingIfNeeded();

        containerLayer.Children.Add(childLayer);

        var childContext = new PaintingContext(childLayer, childPaintBounds ?? estimatedBounds);
        painter(childContext, offset);
        childContext.StopRecordingIfNeeded();
    }

    /// <summary>子の描画へ不透明度を適用する合成レイヤーを追加します。</summary>
    /// <param name="offset">親の座標系における子の描画原点。</param>
    /// <param name="alpha">不透明度。0は完全な透明、255は完全な不透明を表します。</param>
    /// <param name="painter">不透明度レイヤー内へ描画命令を記録する処理。</param>
    /// <param name="oldLayer">再利用する既存レイヤー。新しく生成する場合は<see langword="null"/>です。</param>
    /// <returns>描画内容を保持する不透明度レイヤー。</returns>
    public OpacityLayer PushOpacity(
        Offset offset,
        byte alpha,
        Action<PaintingContext, Offset> painter,
        OpacityLayer? oldLayer = null)
    {
        var layer = oldLayer ?? new OpacityLayer();

        layer.Alpha = alpha;

        PushLayer(layer, painter, offset);

        return layer;
    }

    /// <summary>任意形状で子の描画を切り抜く合成レイヤーを追加します。</summary>
    /// <param name="offset">親の座標系における子の描画原点。</param>
    /// <param name="bounds">子のローカル座標系における推定描画範囲。</param>
    /// <param name="clipPath">子のローカル座標系で定義された切り抜き形状。元の形状は変更しません。</param>
    /// <param name="painter">切り抜きレイヤー内へ描画命令を記録する処理。</param>
    /// <param name="clipBehavior">境界の描画方法。</param>
    /// <param name="oldLayer">再利用する既存レイヤー。新しく生成する場合は<see langword="null"/>です。</param>
    /// <returns>描画内容を保持するパス切り抜きレイヤー。</returns>
    /// <remarks>描画範囲と切り抜き形状を<paramref name="offset"/>だけ移動して親の座標系へ変換します。</remarks>
    public ClipPathLayer PushClipPath(
        Offset offset,
        SKRect bounds,
        SKPath clipPath,
        Action<PaintingContext, Offset> painter,
        Clip clipBehavior = Clip.Antialias,
        ClipPathLayer? oldLayer = null
    )
    {
        var offsetBounds = bounds;
        offsetBounds.Offset(offset);

        var offsetClipPath = new SKPath(clipPath);
        offsetClipPath.Offset(offset);

        var layer = oldLayer ?? new ClipPathLayer(clipPath);

        layer.ClipBehavior = clipBehavior;
        layer.ClipPath = offsetClipPath;

        PushLayer(layer, painter, offset, offsetBounds);

        return layer;
    }

    /// <summary>角丸矩形で子の描画を切り抜く合成レイヤーを追加します。</summary>
    /// <param name="offset">親の座標系における子の描画原点。</param>
    /// <param name="bounds">子のローカル座標系における推定描画範囲。</param>
    /// <param name="clipRect">子のローカル座標系で定義された角丸矩形。</param>
    /// <param name="painter">切り抜きレイヤー内へ描画命令を記録する処理。</param>
    /// <param name="clipBehavior">境界の描画方法。</param>
    /// <param name="oldLayer">再利用する既存レイヤー。新しく生成する場合は<see langword="null"/>です。</param>
    /// <returns>描画内容を保持する角丸矩形切り抜きレイヤー。</returns>
    /// <remarks>切り抜き矩形を<paramref name="offset"/>だけ移動して親の座標系へ変換します。</remarks>
    public ClipRoundRectLayer PushClipRoundRect(
        Offset offset,
        SKRect bounds,
        SKRoundRect clipRect,
        Action<PaintingContext, Offset> painter,
        Clip clipBehavior = Clip.Antialias,
        ClipRoundRectLayer? oldLayer = null)
    {
        var offsetBounds = bounds;
        offsetBounds.Offset(offset);

        var offsetClipRoundRect = clipRect.MakeOffset(offset);

        var layer = oldLayer ?? new ClipRoundRectLayer(offsetClipRoundRect);

        layer.ClipRect = offsetClipRoundRect;
        layer.ClipBehavior = clipBehavior;

        PushLayer(layer, painter, offset, bounds);

        return layer;
    }

    /// <summary>矩形で子の描画を切り抜く合成レイヤーを追加します。</summary>
    /// <param name="offset">親の座標系における子の描画原点。</param>
    /// <param name="clipRect">子のローカル座標系で定義された切り抜き矩形。</param>
    /// <param name="painter">切り抜きレイヤー内へ描画命令を記録する処理。</param>
    /// <param name="clipBehavior">境界の描画方法。</param>
    /// <param name="oldLayer">再利用する既存レイヤー。新しく生成する場合は<see langword="null"/>です。</param>
    /// <returns>描画内容を保持する矩形切り抜きレイヤー。</returns>
    /// <remarks>切り抜き矩形を<paramref name="offset"/>だけ移動して親の座標系へ変換します。</remarks>
    public ClipRectLayer PushClipRect(
        Offset offset,
        SKRect clipRect,
        Action<PaintingContext, Offset> painter,
        Clip clipBehavior = Clip.Antialias,
        ClipRectLayer? oldLayer = null)
    {
        var offsetClipRect = clipRect;
        offsetClipRect.Offset(offset);

        var layer = oldLayer ?? new ClipRectLayer(offsetClipRect);
        layer.ClipRect = offsetClipRect;
        layer.ClipBehavior = clipBehavior;

        PushLayer(layer, painter, offset, offsetClipRect);

        return layer;
    }

    /// <summary>子の描画内容を現在のレイヤーツリーへ記録します。</summary>
    /// <param name="child">描画する子。</param>
    /// <param name="offset">親の座標系における子の描画原点。</param>
    /// <remarks>
    /// 子が再描画境界の場合は現在の画像記録を確定し、子の独立したレイヤーを合成します。
    /// 境界でない場合は子の描画Dirtyを解除して、現在のコンテキストへ直接描画します。
    /// </remarks>
    public void PaintChild(RenderObject child, Offset offset)
    {
        if (child.IsRepaintBoundary)
        {
            StopRecordingIfNeeded();
            CompositeChild(child, offset);
        }
        else
        {
            child.NeedsPaint = false;
            child.Paint(this, offset);
        }
    }

    /// <summary>再描画境界である子のレイヤーを現在のレイヤーツリーへ合成します。</summary>
    /// <param name="child">独立した合成レイヤーを持つ子。</param>
    /// <param name="offset">親の座標系における子の描画原点。</param>
    /// <remarks>
    /// 子が描画Dirtyの場合は先に子のレイヤーを再描画します。
    /// 子のレイヤーが移動レイヤーでない場合は現在のレイヤーツリーへ追加しません。
    /// </remarks>
    public void CompositeChild(RenderObject child, Offset offset)
    {
        if (child.NeedsPaint)
        {
            RepaintCompositedChild(child);
        }

        if (child.Layer is not TransformLayer childTransformLayer) return;

        childTransformLayer.Transform = SKMatrix.CreateTranslation((float)offset.X, (float)offset.Y);
        containerLayer.Children.Add(childTransformLayer);
    }

    /// <summary>再描画境界であるRenderObjectの合成レイヤーを再構築します。</summary>
    /// <param name="child">再描画するRenderObject。</param>
    /// <remarks>
    /// 既存の移動レイヤーがあれば子レイヤーを消去して再利用し、なければ新しく生成して<see cref="RenderObject.Layer"/>へ設定します。
    /// 描画前に対象の描画Dirtyを解除し、対象の原点を基準として描画内容を記録します。
    /// </remarks>
    public static void RepaintCompositedChild(RenderObject child)
    {
        if (child.Layer is not TransformLayer childLayer)
        {
            childLayer = new TransformLayer();
            child.Layer = childLayer;
        }
        else
        {
            childLayer.Children.Clear();
        }

        var childContext = new PaintingContext(childLayer, SKRect.Create(Offset.Zero, size: child.Size));
        child.NeedsPaint = false;
        child.Paint(childContext, Offset.Zero);
        childContext.StopRecordingIfNeeded();
    }
}
