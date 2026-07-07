using FloatSoda.Elements;
using FloatSoda.RenderObjects;

namespace FloatSoda.Widgets.Components;

public sealed record RichText : MultiChildRenderObjectWidget<RenderParagraph>
{
    public required TextSpan Text { get; init; }

    public override RenderParagraph CreateRenderObject() => new() { Text = Text };

    public override void UpdateRenderObject(RenderParagraph renderObject) => renderObject.Text = Text;
}

public sealed record Text(string Data) : StatelessWidget
{
    public override Widget Build(IBuildContext context)
    {
        var text = new TextSpan(Data);

        return new RichText
        {
            Text = text
        };
    }
}