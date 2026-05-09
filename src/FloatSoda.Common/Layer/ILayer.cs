using FloatSoda.Common.Geometries;

namespace FloatSoda.Common.Layer;

public interface ILayer
{
    public Rect PaintBounds { get; }

    void Layout(LayerContext context);
    void Paint(LayerContext context);
}