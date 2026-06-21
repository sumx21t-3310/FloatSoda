using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.RenderObjects;
using FloatSoda.RenderObjects.Layout;

namespace FloatSoda.Widgets.Layout;

public record Align : SingleChildRenderObjectWidget
{
    public Alignment Alignment { get; init; } = Alignment.Center;
    public double? WidthFactor { get; init; } = null;
    public double? HeightFactor { get; init; } = null;


    public override RenderObject CreateRenderObject()
    {
        return new RenderPositionedBox()
        {
            Alignment = Alignment,
            WidthFactor = WidthFactor,
            HeightFactor = HeightFactor
        };
    }
}

public record Center : StatelessWidget
{
    public Widget? Child { get; init; } = null;

    public override Widget Build(IBuildContext context)
    {
        return new Align()
        {
            Child = Child,
            Alignment = Alignment.Center
        };
    }
}