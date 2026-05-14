using System.Diagnostics;
using FloatSoda.Common.Geometries;
using FloatSoda.Common.Layer;
using FloatSoda.Render;
using SkiaSharp;

namespace FloatSoda;

public class RenderPipeline
{
    public RenderView? RenderView { get; set; }

    private static readonly Stopwatch Stopwatch = Stopwatch.StartNew();

    private float speed = Random.Shared.NextSingle();

    public void FlushLayout() => RenderView?.PerformLayout();

    public void FlushPaint()
    {
        if (RenderView == null) return;
        var root = RenderView.Layer;
        var context = new PaintingContext(root, RenderView.Size.And(Offset.Zero));
        RenderView?.Paint(context, Offset.Zero);
        context.StopRecordingIfNeeded();
    }

    public ILayer Render(float width, float height)
    {
        var root = new ContainerLayer();

        var rect = SKRect.Create(0, 0, width, height);
        var leaf = new PictureLayer();
        var recorder = new SKPictureRecorder();
        var canvas = recorder.BeginRecording(rect);

        var paint = new SKPaint
        {
            Color = SKColors.Red
        };

        var sin = ((float)Math.Sin(Stopwatch.Elapsed.TotalSeconds * speed) + 1) / 2f;
        var cos = ((float)Math.Cos(Stopwatch.Elapsed.TotalSeconds * speed) + 1) / 2f;
        canvas.DrawCircle(cos * width, sin * height, 80f, paint);
        leaf.Picture = recorder.EndRecording();

        var opacityLayer = new OpacityLayer { Alpha = 150 };

        var opacityPictureLayer = new PictureLayer();
        var opacityRecorder = new SKPictureRecorder();
        var opacityCanvas = opacityRecorder.BeginRecording(rect);

        opacityCanvas.DrawCircle(0, 0, 60f, paint);
        opacityPictureLayer.Picture = opacityRecorder.EndRecording();

        opacityLayer.Children.Add(opacityPictureLayer);
        root.Children.Add(opacityLayer);

        root.Children.Add(leaf);

        return root;
    }
}