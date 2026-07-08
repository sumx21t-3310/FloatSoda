using FloatSoda.Common.Geometries;
using FloatSoda.Core;
using SkiaSharp;
using Topten.RichTextKit;

namespace FloatSoda.RenderObjects;

public class RenderParagraph : RenderBox, IHasMultiChildrenRenderObject
{
    public MultiChildrenCollection<RenderBox> Children { get; }

    void IHasMultiChildrenRenderObject.AddChild(RenderObject child) => Children.AddErased(child);

    bool IHasMultiChildrenRenderObject.RemoveChild(RenderObject child) => Children.RemoveErased(child);

    public override void SetupParentData(RenderObject child) => child.ParentData = new BoxParentData();

    public override void Attach(RenderPipeline? owner)
    {
        base.Attach(owner);
        Children.Attach(owner);
    }

    public override void Detach()
    {
        base.Detach();
        Children.Detach();
    }

    public override void VisitChildren(Action<RenderObject> visitor) => Children.VisitChildren(visitor);

    public override void RedepthChildren() => VisitChildren(RedepthChild);

    public required TextSpan Text
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            MarkNeedsPaint();
        }
    }

    private readonly TextPainter _textPainter;

    public RenderParagraph()
    {
        _textPainter = new TextPainter(Text);
        Children = new MultiChildrenCollection<RenderBox>(this);
    }

    public override void PerformLayout()
    {
        _textPainter.Layout(Constraints.MinWidth, Constraints.MaxWidth);
        Size = Constraints.Constrain(_textPainter.Size);
    }

    public override void Paint(PaintingContext context, Offset offset) => _textPainter.Paint(context.Canvas, offset);
}

public record TextSpan(string Text)
{
    public Style? Style { get; init; }

    public void Build(TextBlock textBlock, Style defaultStyle) => textBlock.AddText(Text, Style ?? defaultStyle);
}

public class TextPainter(TextSpan text)
{
    public TextSpan Text
    {
        get;
        set
        {
            if (field.Text == value.Text) return;
            field = value;
            MarkNeedsLayout();
        }
    }

    private TextBlock? _textBlock;
    public SKSize Size => new((float)Width, (float)Height);
    public double Width => _textBlock?.MeasuredWidth ?? 0;
    public double Height => _textBlock?.MeasuredHeight ?? 0;

    private void MarkNeedsLayout() => _textBlock = null;

    private Style CreateDefaultStyle() => new()
    {
        FontSize = 30,
        TextColor = SKColors.Black
    };

    private TextBlock CreateTextBlock()
    {
        var textBlock = new TextBlock();
        text.Build(textBlock, CreateDefaultStyle());
        return textBlock;
    }


    public void Layout(double minWidth, double maxWidth)
    {
        _textBlock ??= CreateTextBlock();

        _textBlock.MaxWidth = (float)maxWidth;
        _textBlock.Layout();
    }


    public void Paint(SKCanvas canvas, Offset offset) => _textBlock?.Paint(canvas, offset);
}