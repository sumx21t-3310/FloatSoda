using FloatSoda.Common.Geometries;
using SkiaSharp;

namespace FloatSoda.RenderObjects;

public class RenderImage : RenderProxyBox
{
    public required SKImage Image { get; init; }

    public override void PerformLayout()
    {
        if (Child != null)
        {
            Child.Layout(Constraints);
            Size = Child.Size;
        }
        else
        {
            Size = Constraints.Constrain(new SKSize(Image.Width, Image.Height));
        }
    }

    public override void Paint(PaintingContext context, Offset offset)
    {
        context.Canvas.DrawImage(Image, SKRect.Create(offset, Size));
        Child?.Paint(context, offset);
    }
}