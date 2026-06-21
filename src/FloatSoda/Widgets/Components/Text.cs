using FloatSoda.Elements;
using FloatSoda.RenderObjects;
using FloatSoda.Widgets;

public sealed record RichText : MultiChildRenderObjectWidget<RenderParagraph>
{
    public required TextSpan Text { get; init; }
    public override RenderParagraph CreateRenderObject() => new(Text);
}

public sealed record Text : StatelessWidget
{
    public override Widget Build(IBuildContext context)
    {
        throw new NotImplementedException();
    }
}