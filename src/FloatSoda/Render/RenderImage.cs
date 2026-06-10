using FloatSoda.Common.Geometries;
using FloatSoda.Geometrics;
using SkiaSharp;

namespace FloatSoda.Render;

public class RenderImage : RenderProxyBox
{
    public required SKImage Image { get; init; }

    public override void Layout(BoxConstraints constraints)
    {
        if (Child != null)
        {
            Child.Layout(constraints);
            Size = Child.Size;
        }
        else
        {
            Size = constraints.Constrain(new SKSize(Image.Width, Image.Height));
        }
    }

    public override void Paint(PaintingContext context, Offset offset)
    {
        context.Canvas.DrawImage(Image, SKRect.Create(offset, Size));
        Child?.Paint(context, offset);
    }
}