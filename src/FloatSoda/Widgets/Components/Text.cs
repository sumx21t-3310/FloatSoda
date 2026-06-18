using FloatSoda.Elements;
using FloatSoda.Render;
using FloatSoda.Widgets;

public sealed record RichText : MultiChildRenderObjectWidget
{
    public required TextSpan Text { get; init; }
    public override RenderObject CreateRenderObject() => new RenderParagraph(Text);
}

public sealed record Text : StatelessWidget
{
    public override Widget Build(IBuildContext context)
    {
        throw new NotImplementedException();
    }
}