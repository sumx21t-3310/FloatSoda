using FloatSoda.Core;
using FloatSoda.RenderObjects;

namespace FloatSoda.Widgets.Paint;

public record Image : SingleChildRenderObjectWidget<RenderImage>
{
    public required ImageProvider ImageProvider { get; init; }

    public override RenderImage CreateRenderObject()
    {
        return new RenderImage()
        {
            Image = ImageProvider.Load()
        };
    }
}