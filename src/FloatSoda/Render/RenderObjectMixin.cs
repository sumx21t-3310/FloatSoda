namespace FloatSoda.Render;

public interface IHasSingleChildRenderObjectBase
{
    void SetChildObject(RenderObject child);
}

public interface IHasMultiChildrenRenderObjectBase
{
    void Insert(RenderObject child);
}

public interface IHasSingleChildRenderObject<T> : IHasSingleChildRenderObjectBase
    where T : RenderObject
{
    T? Child { get; protected set; }

    RenderObject ThisRef { get; }

    public void SetChild(T child)
    {
        Child = child;
        ThisRef.AdoptChild(child);
    }

    void IHasSingleChildRenderObjectBase.SetChildObject(RenderObject child)
    {
        if (child is T typed) SetChild(typed);
    }

    void RedepthChild(Action<RenderObject> callback)
    {
        if (Child != null)
        {
            callback(Child);
        }
    }
}

public interface IHasMultiChildrenRenderObject<T> : IHasMultiChildrenRenderObjectBase
    where T : RenderObject
{
    List<T> Children { get; }
    RenderObject ThisRef { get; }

    void IHasMultiChildrenRenderObjectBase.Insert(RenderObject child)
    {
        if (child is T typed) Insert(typed);
    }

    void Insert(T child)
    {
        ThisRef.AdoptChild(child);
        Children.Add(child);
    }

    void VisitChildren(Action<RenderObject> visitor) => Children.ForEach(visitor);

    public void RedepthChildren(Action<RenderObject> callback)
    {
        foreach (var child in Children)
        {
            callback(child);
        }
    }
}
