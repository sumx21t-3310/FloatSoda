using FloatSoda.Common.Geometries;
using FloatSoda.Common.Layer;
using FloatSoda.Geometrics;
using SkiaSharp;

namespace FloatSoda.Render;

public class RenderView(float width, float height) : RenderObject
{
    public override SKSize Size { get; protected set; } = new(width, height);

    public RenderBox? Child { get; set; }
    public ContainerLayer Layer { get; } = new TransformLayer();


    public void PerformLayout() => Layout(BoxConstraints.Tight(Size));
    public override void Layout(BoxConstraints constraints) => Child?.Layout(constraints);

    public override void Paint(PaintingContext context, Offset offset) => Child?.Paint(context, offset);
}