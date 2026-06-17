using FloatSoda.Core;
using FloatSoda.Render;

namespace FloatSoda.Widgets.Paint;

public record Image : SingleChildRenderObjectWidget
{
    public required ImageProvider ImageProvider { get; init; }

    public override RenderObject CreateRenderObject()
    {
        return new RenderImage()
        {
            Image = ImageProvider.Load()
        };
    }
}