using System.Numerics;
using FloatSoda.Render;
using SkiaSharp;

var imageRenderer = new ImageRenderer(new Size(400, 400));

var root = new BoxElement()
{
    Color = SKColors.Red,
    Size = new Size(300, 200),
    Child = new BoxElement()
    {
        Color = SKColors.Aqua,
        Size = new Size(100, 100),
        Position = new Vector2(200, 200),
    }
};

var savePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "output.png");

imageRenderer.RenderImage(root, savePath);


public class ImageRenderer(Size imageSize)
{
    private readonly SKImageInfo _info = new((int)imageSize.Width, (int)imageSize.Height);

    public void RenderImage(Element root, string savePath)
    {
        using var surface = SKSurface.Create(_info);

        surface.Canvas.Clear(SKColors.Transparent);
        var renderContext = RenderContext.Create(surface);

        root.Draw(renderContext);

        var image = surface.Snapshot();

        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.Create(savePath);
        data.SaveTo(stream);
    }
}