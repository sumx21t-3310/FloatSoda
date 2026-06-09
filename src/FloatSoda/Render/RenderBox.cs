using FloatSoda.Common.Geometries;
using FloatSoda.Geometrics;
using FloatSoda.Render.Mixin;
using SkiaSharp;

namespace FloatSoda.Render;

public abstract class RenderBox : RenderObject
{
    public override SKSize Size { get; protected set; } = SKSize.Empty;
}

public abstract class RenderProxyBox : RenderBox, IRenderObjectWithChild<RenderBox>
{
    public RenderBox? Child { get; set; } = null;

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