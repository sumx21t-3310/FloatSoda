using FloatSoda.RenderObjects;
using FloatSoda.Widgets;

namespace FloatSoda.Elements;

public abstract class RenderObjectElement : Element
{
    private RenderObjectElement? _ancestorRenderObjectElement;


    public override void AttachRenderObject()
    {
        _ancestorRenderObjectElement = FindAncestorRenderObjectElement();
        _ancestorRenderObjectElement?.InsertRenderObjectChild(RenderObject);
    }


    public override void DetachRenderObject()
    {
        if (_ancestorRenderObjectElement == null) return;
        _ancestorRenderObjectElement.RemoveRenderObjectChild(RenderObject);
        _ancestorRenderObjectElement = null;
    }


    public virtual void InsertRenderObjectChild(RenderObject? child) { }


    public virtual void RemoveRenderObjectChild(RenderObject? child) { }


    public RenderObjectElement? FindAncestorRenderObjectElement()
    {
        var ancestor = Parent;

        while (ancestor != null && ancestor is not RenderObjectElement)
        {
            ancestor = ancestor.Parent;
        }

        return ancestor as RenderObjectElement;
    }

    protected List<Element> UpdateChildren(
        List<Element> oldChildren,
        List<Widget> newWidgets,
        HashSet<Element>? forgottenChildren = null)
    {
        Element? ReplaceWithNullIfForgotten(Element child)
            => forgottenChildren != null && forgottenChildren.Contains(child) ? null : child;

        int newChildrenTop = 0;
        int oldChildrenTop = 0;
        int newChildrenBottom = newWidgets.Count - 1;
        int oldChildrenBottom = oldChildren.Count - 1;

        var newChildren = new Element?[newWidgets.Count];

        // 1. 前方から一致する部分を進める
        while (oldChildrenTop <= oldChildrenBottom && newChildrenTop <= newChildrenBottom)
        {
            var oldChild = ReplaceWithNullIfForgotten(oldChildren[oldChildrenTop]);
            var newWidget = newWidgets[newChildrenTop];
            if (oldChild == null || !Widget.CanUpdate(oldChild.Widget!, newWidget)) break;

            var newChild = UpdateChild(oldChild, newWidget);
            newChildren[newChildrenTop] = newChild;
            newChildrenTop++;
            oldChildrenTop++;
        }

        // 2. 後方から一致する部分を確認（bottomだけ動かす。まだ書き戻さない）
        while (oldChildrenTop <= oldChildrenBottom && newChildrenTop <= newChildrenBottom)
        {
            var oldChild = ReplaceWithNullIfForgotten(oldChildren[oldChildrenBottom]);
            var newWidget = newWidgets[newChildrenBottom];
            if (oldChild == null || !Widget.CanUpdate(oldChild.Widget!, newWidget)) break;

            oldChildrenBottom--;
            newChildrenBottom--;
        }

        // 3. 中間範囲：古い子をkeyでインデックス化
        Dictionary<IKey, Element>? oldKeyedChildren = null;
        if (oldChildrenTop <= oldChildrenBottom)
        {
            oldKeyedChildren = new Dictionary<IKey, Element>();
            while (oldChildrenTop <= oldChildrenBottom)
            {
                var oldChild = ReplaceWithNullIfForgotten(oldChildren[oldChildrenTop]);
                if (oldChild != null)
                {
                    var key = oldChild.Widget?.Key;
                    if (key != null)
                    {
                        oldKeyedChildren[key] = oldChild;
                    }
                    else
                    {
                        DeactivateChild(oldChild); // keyなしは再利用できないので即破棄
                    }
                }

                oldChildrenTop++;
            }
        }

        // 4. 新しいWidgetをkeyで探して再利用、なければ新規作成
        while (newChildrenTop <= newChildrenBottom)
        {
            var newWidget = newWidgets[newChildrenTop];
            Element? oldChild = null;

            var key = newWidget.Key;
            if (key != null && oldKeyedChildren != null && oldKeyedChildren.TryGetValue(key, out var matched))
            {
                if (Widget.CanUpdate(matched.Widget!, newWidget))
                {
                    oldChild = matched;
                    oldKeyedChildren.Remove(key);
                }
            }

            var newChild = UpdateChild(oldChild, newWidget);
            newChildren[newChildrenTop] = newChild;
            newChildrenTop++;
        }

        // 5. 後方一致していた部分を書き戻す
        newChildrenBottom = newWidgets.Count - 1;
        oldChildrenBottom = oldChildren.Count - 1;
        while (oldChildrenTop <= oldChildrenBottom)
        {
            var oldChild = oldChildren[oldChildrenTop];
            var newWidget = newWidgets[newChildrenTop];
            var newChild = UpdateChild(oldChild, newWidget);
            newChildren[newChildrenTop] = newChild;
            newChildrenTop++;
            oldChildrenTop++;
        }

        // 6. 再利用されずに残った古い要素を破棄
        if (oldKeyedChildren != null)
        {
            foreach (var remaining in oldKeyedChildren.Values)
            {
                DeactivateChild(remaining);
            }
        }

        return newChildren.Where(c => c != null).Select(c => c!).ToList();
    }
}

public abstract class RenderObjectElement<T> : RenderObjectElement where T : RenderObject
{
    public abstract override RenderObject? RenderObject { get; protected set; }
    private RenderObjectWidget<T>? WidgetCascaded => Widget as RenderObjectWidget<T>;


    public override void Mount(Element? parent)
    {
        base.Mount(parent);
        if (Widget is RenderObjectWidget<T> renderObjectWidget) RenderObject = renderObjectWidget.CreateRenderObject();
        AttachRenderObject();
        Dirty = false;
    }


    public override void Update(Widget newWidget)
    {
        base.Update(newWidget);
        PerformRebuild();
    }


    public override void PerformRebuild()
    {
        WidgetCascaded?.UpdateRenderObject(RenderObject as T);
        Dirty = false;
    }
}