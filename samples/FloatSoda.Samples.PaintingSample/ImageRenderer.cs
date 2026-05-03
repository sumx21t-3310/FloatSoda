using FloatSoda.Engine.Layer;
using FloatSoda.Engine.Render;
using SkiaSharp;

namespace FloatSoda.Samples.PaintingSample;

public class ImageRenderer
{
    public void RenderImage(ILayer root, Size imageSize, string savePath)
    {
        SKImageInfo info = new((int)imageSize.Width, (int)imageSize.Height);
        using var surface = SKSurface.Create(info);

        surface.Canvas.Clear(SKColors.Transparent);
        var renderContext = LayerContext.Create(surface);

        root.Paint(renderContext);

        var image = surface.Snapshot();

        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.Create(savePath);
        data.SaveTo(stream);
    }
}