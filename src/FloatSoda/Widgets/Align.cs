using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.Render;
using FloatSoda.Render.Layout;

namespace FloatSoda.Widgets;

public record Align : SingleChildRenderObjectWidget
{
    public virtual Alignment Alignment { get; init; } = Alignment.Center;
    public double? WidthFactor { get; init; } = null;
    public double? HeightFactor { get; init; } = null;

    public override Element CreateElement()
    {
        throw new NotImplementedException();
    }

    public override RenderObject CreateRenderObject() => new RenderPositionedBox
    {
        Alignment = Alignment,
        WidthFactor = WidthFactor,
        HeightFactor = HeightFactor
    };
}