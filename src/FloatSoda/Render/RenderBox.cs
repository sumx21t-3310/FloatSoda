using FloatSoda.Common.Geometries;
using FloatSoda.Geometrics;
using SkiaSharp;

namespace FloatSoda.Render;

public abstract class RenderBox : RenderObject
{
    public override SKSize Size { get; protected set; } = SKSize.Empty;
}

public abstract class RenderProxyBox : RenderBox
{
    public RenderBox? Child { get; init; } = null;

    public override void Layout(BoxConstraints constraints)
    {
        if (Child != null)
        {
            Child.Layout(constraints);
            Size = Child.Size;
        }
        else
        {
            Size = constraints.Smallest;
        }
    }

    public override void Paint(PaintingContext context, Offset offset) => Child?.Paint(context, offset);
}