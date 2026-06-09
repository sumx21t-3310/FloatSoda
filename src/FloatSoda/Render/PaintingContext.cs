namespace FloatSoda.Render;

using Common.Geometries;
using Common.Layer;
using SkiaSharp;

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
}