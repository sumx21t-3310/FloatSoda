using FloatSoda.Engine.Painting;
using FloatSoda.Samples.PaintingSample;
using SkiaSharp;

var imageRenderer = new ImageRenderer();

var root = new ContainerLayer
{
    Children =
    {
        new PaintLayer { Color = SKColors.Tomato, Size = new Size(1000, 1000) },
        new PaintLayer { Color = SKColors.CornflowerBlue, Size = new Size(600, 1000) },
        new PaintLayer { Color = SKColors.AliceBlue, Size = new Size(100, 1000) }
    }
};

var savePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "output.png");

imageRenderer.RenderImage(root, new Size(1000, 1000), savePath);