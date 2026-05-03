using FloatSoda.Engine.Layer;
using FloatSoda.Samples.PaintingSample;
using SkiaSharp;

var imageRenderer = new ImageRenderer();

var root = CreateLayerTree(1000, 1000);

var savePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "output.png");

imageRenderer.RenderImage(root, new Size(1000, 1000), savePath);

ILayer CreateLayerTree(float width, float height)
{
    var root = new ContainerLayer();

    var rect = Rect.LTWH(0, 0, width, height);
    var leef = new PictureLayer();
    var recorder = new SKPictureRecorder();
    var canvas = recorder.BeginRecording(rect);

    var paint = new SKPaint()
    {
        Color = SKColors.Red
    };

    canvas.DrawCircle(Random.Shared.NextSingle() * width, Random.Shared.NextSingle() * height, 40f, paint);
    leef.Picture = recorder.EndRecording();
    
    root.Children.Add(leef);

    return root;
}