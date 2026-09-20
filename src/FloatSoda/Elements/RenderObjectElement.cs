using FloatSoda.RenderObjects;
using FloatSoda.Widgets;

namespace FloatSoda.Elements;

/// <summary>
/// 対応するRenderObjectを祖先のRenderObjectツリーへ接続するElementの基底クラスです。
/// </summary>
/// <seealso cref="RenderObject"/>
public abstract class RenderObjectElement : Element
{
    private RenderObjectElement? _ancestorRenderObjectElement;


    /// <summary>
    /// 最近傍の祖先RenderObjectElementへ、このElementのRenderObjectを子として接続します。
    /// </summary>
    public override void AttachRenderObject()
    {
        _ancestorRenderObjectElement = FindAncestorRenderObjectElement();
        _ancestorRenderObjectElement?.InsertRenderObjectChild(RenderObject, Slot);
        ApplyParentData();
    }

    /// <summary>
    /// slotを更新し、接続先の祖先へ、このElementのRenderObjectを新しい位置へ移動するよう伝えます。
    /// </summary>
    /// <param name="newSlot">新しいslot。</param>
    protected internal override void UpdateSlot(object? newSlot)
    {
        var oldSlot = Slot;
        base.UpdateSlot(newSlot);

        if (RenderObject is { } renderObject)
        {
            _ancestorRenderObjectElement?.MoveRenderObjectChild(renderObject, oldSlot, newSlot);
        }
    }

    private void ApplyParentData()
    {
        if (RenderObject is not { } renderObject) return;

        var ancestor = Parent;
        while (ancestor is not null && ancestor != _ancestorRenderObjectElement)
        {
            if (ancestor is IParentDataElement parentDataElement)
            {
                parentDataElement.ApplyParentData(renderObject);
            }

            ancestor = ancestor.Parent;
        }
    }


    /// <summary>
    /// 接続先として記録された祖先RenderObjectElementから、このElementのRenderObjectを切り離します。
    /// </summary>
    public override void DetachRenderObject()
    {
        if (_ancestorRenderObjectElement == null) return;
        _ancestorRenderObjectElement.RemoveRenderObjectChild(RenderObject);
        _ancestorRenderObjectElement = null;
    }


    /// <summary>
    /// このElementのRenderObjectへ子RenderObjectを挿入します。
    /// </summary>
    /// <param name="child">挿入する子RenderObject。子を持たないことを表す場合は<see langword="null"/>。</param>
    public virtual void InsertRenderObjectChild(RenderObject? child) { }

    /// <summary>
    /// このElementのRenderObjectへ、slotが示す位置に子RenderObjectを挿入します。
    /// </summary>
    /// <param name="child">挿入する子RenderObject。子を持たないことを表す場合は<see langword="null"/>。</param>
    /// <param name="slot">子の<see cref="Element.Slot"/>。</param>
    /// <remarks>基底実装はslotを使わず、<see cref="InsertRenderObjectChild(RenderObject?)"/>を呼び出します。</remarks>
    public virtual void InsertRenderObjectChild(RenderObject? child, object? slot) => InsertRenderObjectChild(child);

    /// <summary>
    /// このElementのRenderObjectの中で、子RenderObjectを新しいslotが示す位置へ移動します。
    /// </summary>
    /// <param name="child">移動する子RenderObject。</param>
    /// <param name="oldSlot">移動前のslot。</param>
    /// <param name="newSlot">移動後のslot。</param>
    /// <remarks>基底実装は何もしません。子の順序を持つElementが上書きします。</remarks>
    public virtual void MoveRenderObjectChild(RenderObject child, object? oldSlot, object? newSlot) { }


    /// <summary>
    /// このElementのRenderObjectから子RenderObjectを取り除きます。
    /// </summary>
    /// <param name="child">取り除く子RenderObject。対象がない場合は<see langword="null"/>。</param>
    public virtual void RemoveRenderObjectChild(RenderObject? child) { }


    /// <summary>
    /// Elementツリーを親方向へ探索し、最も近いRenderObjectElementを取得します。
    /// </summary>
    /// <returns>
    /// 最も近い祖先のRenderObjectElement。該当する祖先が存在しない場合は<see langword="null"/>。
    /// </returns>
    public RenderObjectElement? FindAncestorRenderObjectElement()
    {
        var ancestor = Parent;

        while (ancestor != null && ancestor is not RenderObjectElement)
        {
            ancestor = ancestor.Parent;
        }

        return ancestor as RenderObjectElement;
    }

    /// <summary>
    /// 複数の子Elementを新しいWidget一覧へ更新し、型とキーが一致する子を再利用します。
    /// </summary>
    /// <param name="oldChildren">更新前の子Element一覧。</param>
    /// <param name="newWidgets">更新後に子として構成するWidget一覧。</param>
    /// <param name="forgottenChildren">
    /// 再利用候補から除外する子Elementの集合。除外する子がない場合は<see langword="null"/>。
    /// </param>
    /// <returns>新しいWidget一覧と同じ順序で構成された更新後の子Element一覧。</returns>
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

        // slotは「自分のindexと、直前の兄弟Element」の組。直前の兄弟のRenderObjectの次が挿入位置になる。
        Element? previousChild = null;

        // 1. 前方から一致する部分を進める
        while (oldChildrenTop <= oldChildrenBottom && newChildrenTop <= newChildrenBottom)
        {
            var oldChild = ReplaceWithNullIfForgotten(oldChildren[oldChildrenTop]);
            var newWidget = newWidgets[newChildrenTop];
            if (oldChild == null || !Widget.CanUpdate(oldChild.Widget!, newWidget)) break;

            var newChild = UpdateChild(oldChild, newWidget, new IndexedSlot(newChildrenTop, previousChild));
            newChildren[newChildrenTop] = newChild;
            previousChild = newChild;
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

            var newChild = UpdateChild(oldChild, newWidget, new IndexedSlot(newChildrenTop, previousChild));
            newChildren[newChildrenTop] = newChild;
            previousChild = newChild;
            newChildrenTop++;
        }

        // 5. 後方一致していた部分を書き戻す
        newChildrenBottom = newWidgets.Count - 1;
        oldChildrenBottom = oldChildren.Count - 1;
        while (oldChildrenTop <= oldChildrenBottom)
        {
            var oldChild = oldChildren[oldChildrenTop];
            var newWidget = newWidgets[newChildrenTop];
            var newChild = UpdateChild(oldChild, newWidget, new IndexedSlot(newChildrenTop, previousChild));
            newChildren[newChildrenTop] = newChild;
            previousChild = newChild;
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

/// <summary>
/// 指定した型のRenderObjectを生成・更新するRenderObjectWidgetに対応するElementの基底クラスです。
/// </summary>
/// <typeparam name="T">このElementが管理するRenderObjectの型。</typeparam>
/// <seealso cref="RenderObjectWidget{T}"/>
public abstract class RenderObjectElement<T> : RenderObjectElement where T : RenderObject
{
    /// <summary>
    /// このElementが管理するRenderObjectを取得します。
    /// RenderObjectがまだ生成されていない場合は<see langword="null"/>です。
    /// </summary>
    public abstract override RenderObject? RenderObject { get; protected set; }
    private RenderObjectWidget<T>? WidgetCascaded => Widget as RenderObjectWidget<T>;


    /// <summary>
    /// Elementツリーへ接続し、対応するWidgetからRenderObjectを生成してRenderObjectツリーへ接続します。
    /// </summary>
    /// <param name="parent">接続先の親Element。ルートとして接続する場合は<see langword="null"/>。</param>
    public override void Mount(Element? parent)
    {
        base.Mount(parent);
        if (Widget is RenderObjectWidget<T> renderObjectWidget) RenderObject = renderObjectWidget.CreateRenderObject();
        AttachRenderObject();
        Dirty = false;
    }


    /// <summary>
    /// 管理するWidgetを置き換え、その構成を既存のRenderObjectへ直ちに反映します。
    /// </summary>
    /// <param name="newWidget">同じRenderObjectを引き継ぐ新しいWidget。</param>
    public override void Update(Widget newWidget)
    {
        base.Update(newWidget);
        PerformRebuild();
    }


    /// <summary>
    /// 現在のWidgetの構成を管理対象のRenderObjectへ反映し、再構築が必要な状態を解除します。
    /// </summary>
    public override void PerformRebuild()
    {
        WidgetCascaded?.UpdateRenderObject(RenderObject as T);
        Dirty = false;
    }
}
