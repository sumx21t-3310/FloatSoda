using FloatSoda.Common.Geometries;
using FloatSoda.Geometrics;
using SkiaSharp;

namespace FloatSoda.Render;

public abstract class RenderObject
{
    public IParentData? ParentData { get; set; }

    public abstract SKSize Size { get; protected set; }

    public abstract void Layout(BoxConstraints constraints);
    public abstract void Paint(PaintingContext context, Offset offset);
}