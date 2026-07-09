using FloatSoda.RenderObjects;
using FloatSoda.Widgets;

namespace FloatSoda.Elements;

/// <summary>
/// <see cref="WindowWidget"/> の Element。マウント・更新時にウィンドウ宣言の
/// <see cref="WindowWidget.Size"/> をルートの <see cref="RenderView.FixedSize"/> へ反映する。
/// </summary>
public class WindowElement : InheritedElement
{
    public override void Mount(Element? parent)
    {
        base.Mount(parent);
        ApplySizeToRenderView();
    }

    public override void Update(Widget newWidget)
    {
        base.Update(newWidget);
        ApplySizeToRenderView();
    }

    private void ApplySizeToRenderView()
    {
        for (var ancestor = Parent; ancestor != null; ancestor = ancestor.Parent)
        {
            if (ancestor is not RenderObjectElement { RenderObject: RenderView view }) continue;

            view.FixedSize = (Widget as WindowWidget)?.Size;
            return;
        }
    }
}
