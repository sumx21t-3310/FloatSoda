using FloatSoda.Render;
using FloatSoda.Widgets;

namespace FloatSoda.Elements;

public class MultiChildRenderObjectElement : RenderObjectElement
{
    public MultiChildRenderObjectWidget WidgetCascade => Widget;
    public required MultiChildRenderObjectWidget Widget { get; set; }

    private List<Element> Children { get; set; } = [];

    public override void Mount(Element? parent)
    {
        base.Mount(parent);

        Children = WidgetCascade.Children.Select(InflateWidget).ToList();
    }

    protected override void InsertRenderObjectChild(RenderObject child)
    {
        (RenderObject as IHasMultiChildrenRenderObject<RenderObject>)?.Insert(child);
    }
}