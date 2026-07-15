using FloatSoda.Abstractions.Geometries;
using FloatSoda.Rendering.Layers;
using SkiaSharp;

namespace FloatSoda.RenderObjects;

public class PaintingContext(ContainerLayer containerLayer, SKRect estimatedBounds)
{
    private PictureLayer? _currentLayer;
    private SKPictureRecorder? _recorder;
    private SKCanvas? _canvas;
    private bool IsRecording => _canvas != null;

    public SKCanvas Canvas
    {
        get
        {
            if (_canvas == null) StartRecording();
            return _canvas!;
        }
    }

    private void StartRecording()
    {
        _currentLayer = new PictureLayer();
        _recorder = new SKPictureRecorder();
        containerLayer.Children.Add(_currentLayer);
        _canvas = _recorder.BeginRecording(estimatedBounds);
    }

    public void StopRecordingIfNeeded()
    {
        if (!IsRecording) return;

        _currentLayer?.Picture = _recorder?.EndRecording();
        _currentLayer = null;
        _recorder = null;
        _canvas = null;
    }

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