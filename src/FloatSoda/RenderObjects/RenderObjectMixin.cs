using System.Collections;
using FloatSoda.Core;

namespace FloatSoda.RenderObjects;

/// <summary>
/// 単一の子を保持するRenderObjectが実装するインターフェース。Element境界からの子の差し替えに使う。
/// </summary>
public interface IHasSingleChildRenderObject
{
    /// <summary>保持している子。</summary>
    RenderObject? Child { get; set; }
}

/// <summary>
/// 複数の子を保持するRenderObjectが実装するインターフェース。Element境界からの子の挿入に使う。
/// </summary>
public interface IHasMultiChildrenRenderObject
{
    /// <summary>子を末尾に追加する。</summary>
    void AddChild(RenderObject child);
}

/// <summary>
/// 単一の子の保持をコンポジションで提供するコンテナ。子の差し替え時にownerへのAdopt/Dropを行い、
/// ライフサイクル(Attach/Detach/Visit)の転送ヘルパーを提供する。
/// </summary>
public class SingleChildContainer<T>(RenderObject owner) where T : RenderObject
{
    /// <summary>保持している子。差し替え時は旧子をDropし新子をAdoptする。</summary>
    public T? Child
    {
        get;
        set
        {
            if (field != null) owner.DropChild(field);
            field = value;
            if (value != null) owner.AdoptChild(value);
        }
    }

    /// <summary>子をRenderPipelineへアタッチする。ownerのAttachから呼ぶ。</summary>
    public void Attach(RenderPipeline? pipeline) => Child?.Attach(pipeline);

    /// <summary>子をデタッチする。ownerのDetachから呼ぶ。</summary>
    public void Detach() => Child?.Detach();

    /// <summary>子が存在すればvisitorを適用する。</summary>
    public void VisitChildren(Action<RenderObject> visitor)
    {
        if (Child != null) visitor(Child);
    }
}

/// <summary>
/// 複数の子の保持をコンポジションで提供するコレクション。追加・削除時にownerへのAdopt/Dropを行い、
/// ライフサイクル(Attach/Detach/Visit)の転送ヘルパーを提供する。
/// </summary>
public class MultiChildrenCollection<T>(RenderObject owner) : IReadOnlyList<T> where T : RenderObject
{
    private readonly List<T> _children = [];

    public int Count => _children.Count;

    public T this[int index] => _children[index];

    /// <summary>子を末尾に追加する。</summary>
    public void Add(T child)
    {
        owner.AdoptChild(child);
        _children.Add(child);
    }

    /// <summary>子を取り除く。</summary>
    public bool Remove(T child)
    {
        if (!_children.Remove(child)) return false;

        owner.DropChild(child);
        return true;
    }

    /// <summary>全ての子を取り除く。</summary>
    public void Clear()
    {
        foreach (var child in _children)
        {
            owner.DropChild(child);
        }

        _children.Clear();
    }

    /// <summary>全ての子をRenderPipelineへアタッチする。ownerのAttachから呼ぶ。</summary>
    public void Attach(RenderPipeline? pipeline)
    {
        foreach (var child in _children)
        {
            child.Attach(pipeline);
        }
    }

    /// <summary>全ての子をデタッチする。ownerのDetachから呼ぶ。</summary>
    public void Detach()
    {
        foreach (var child in _children)
        {
            child.Detach();
        }
    }

    /// <summary>全ての子にvisitorを適用する。</summary>
    public void VisitChildren(Action<RenderObject> visitor)
    {
        foreach (var child in _children)
        {
            visitor(child);
        }
    }

    public IEnumerator<T> GetEnumerator() => _children.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>
/// <see cref="MultiChildrenCollection{T}"/>向けの拡張メソッド。
/// </summary>
public static class MultiChildrenCollectionExtensions
{
    /// <summary>保持している子を指定した子の一覧で置き換える。</summary>
    public static void Swap<T>(this MultiChildrenCollection<T> collection, ReadOnlySpan<T> children)
        where T : RenderObject
    {
        collection.Clear();

        foreach (var child in children)
        {
            collection.Add(child);
        }
    }
}
