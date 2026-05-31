namespace FloatSoda.Render;

using Common.Geometries;
using Common.Layer;
using SkiaSharp;

public delegate void PaintingContextCallback(PaintingContext context, Offset offset);

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

    public void PushLayer(ContainerLayer childLayer, PaintingContextCallback painter, Offset offset,
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

    public ClipPathLayer ClipPath()
    {
        throw new NotImplementedException();
    }

    public ClipRoundRectLayer PushClipRoundRect(
        Offset offset,
        SKRect clipRect,
        SKRect bounds,
        PaintingContextCallback painter,
        Clip clipBehavior = Clip.Antialias, 
        ClipRoundRectLayer? clipPathLayer = null)
    {
        throw new NotImplementedException();
    }

    public ClipRectLayer PushClipRect(
        Offset offset,
        SKRect clipRect,
        PaintingContextCallback painter,
        Clip clipBehavior = Clip.Antialias,
        ClipRectLayer? oldLayer = null)
    {
        var offsetClipRect = clipRect.MakeOffset(offset);
        throw new NotImplementedException();
    }
}