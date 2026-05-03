namespace FloatSoda.Engine.Layer;

public interface ILayer
{
    public Rect PaintBounds { get; }

    void Layout(LayerContext context);
    void Paint(LayerContext context);
}