using SkiaSharp;

namespace FloatSoda.Engine.Layer;

public class PictureLayer : ILayer
{
    public SKPicture? Picture { get; set; }

    public Rect PaintBounds { get; private set; }


    public void Layout(LayerContext context)
    {
        PaintBounds = Picture?.CullRect ?? new Rect();
    }

    public void Paint(LayerContext context)
    {
        Picture?.Playback(context.Canvas);
    }
}