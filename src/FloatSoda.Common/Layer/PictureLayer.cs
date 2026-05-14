using SkiaSharp;

namespace FloatSoda.Common.Layer;

public class PictureLayer : ILayer
{
    public SKPicture? Picture { get; set; }

    public SKRect PaintBounds { get; private set; }


    public void Layout(LayerContext context)
    {
        PaintBounds = Picture?.CullRect ?? new SKRect();
    }

    public void Paint(LayerContext context)
    {
        Picture?.Playback(context.Canvas);
    }
}