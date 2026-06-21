using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.RenderObjects;
using FloatSoda.RenderObjects.Layout;

namespace FloatSoda.Widgets.Layout;

public sealed record Flex : MultiChildRenderObjectWidget
{
    public Axis Direction { get; init; } = Axis.Vertical;
    public MainAxisAlignment MainAxisAlignment { get; init; } = MainAxisAlignment.Start;

    public MainAxisSize MainAxisSize { get; init; } = MainAxisSize.Max;
    public CrossAxisAlignment CrossAxisAlignment { get; init; } = CrossAxisAlignment.Start;
    public VerticalDirection VerticalDirection { get; init; } = VerticalDirection.Down;

    public override RenderObject CreateRenderObject()
    {
        return new RenderFlex()
        {
            Direction = Direction,
            MainAxisAlignment = MainAxisAlignment,
            MainAxisSize = MainAxisSize,
            CrossAxisAlignment = CrossAxisAlignment,
            VerticalDirection = VerticalDirection,
        };
    }
}

public abstract record FlexWrapper(Axis Direction) : StatelessWidget
{
    public List<Widget> Children { get; init; } = [];
    public MainAxisAlignment MainAxisAlignment { get; init; } = MainAxisAlignment.Start;
    public CrossAxisAlignment CrossAxisAlignment { get; init; } = CrossAxisAlignment.Start;
    public MainAxisSize MainAxisSize { get; init; } = MainAxisSize.Max;

    public override Widget Build(IBuildContext context)
    {
        return new Flex()
        {
            MainAxisAlignment = MainAxisAlignment,
            CrossAxisAlignment = CrossAxisAlignment,
            MainAxisSize = MainAxisSize,
            Direction = Direction,
            Children = Children,
        };
    }
}

public sealed record Column() : FlexWrapper(Axis.Vertical);

public sealed record Row() : FlexWrapper(Axis.Horizontal);