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

    /// <summary>指定した子を取り除く。取り除けたら true。</summary>
    bool RemoveChild(RenderObject child);
}


/// <summary>
/// 単一の子の保持をコンポジションで提供するコンテナ。子の差し替え時にownerへのAdopt/Dropを行い、
/// ライフサイクル(Attach/Detach/Visit)の転送ヘルパーを提供する。
/// </summary>
/// <param name="owner">子を所有し、追加と削除の通知を受け取るRenderObject。</param>
public class SingleChildContainer<T>(RenderObject owner) where T : RenderObject
{
    /// <summary>保持している子。差し替え時は旧子をDropし新子をAdoptする。</summary>
    /// <remarks>
    /// 子を差し替えると、所有者をLayout Dirtyとしてマークします。
    /// 同じ子を再設定した場合も、子の取り外しと追加が行われるためLayout Dirtyとなります。
    /// </remarks>
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
/// <param name="owner">子を所有し、追加と削除の通知を受け取るRenderObject。</param>
public class MultiChildrenCollection<T>(RenderObject owner) : ICollection<T> where T : RenderObject
{
    private readonly List<T> _children = [];

    /// <summary>保持している子の数を取得する。</summary>
    public int Count => _children.Count;

    /// <summary>コレクションが読み取り専用かどうかを取得する。</summary>
    public bool IsReadOnly => false;

    /// <summary>子を末尾に追加する。</summary>
    /// <remarks>
    /// 所有者をLayout Dirtyとしてマークし、次のパイプライン更新時に再レイアウトを要求します。
    /// </remarks>
    public void Add(T child)
    {
        owner.AdoptChild(child);
        _children.Add(child);
    }

    /// <summary>保持している子を指定した配列へコピーする。</summary>
    /// <param name="array">子をコピーする配列。</param>
    /// <param name="arrayIndex">コピーを開始する配列の位置。</param>
    public void CopyTo(T[] array, int arrayIndex) => _children.CopyTo(array, arrayIndex);

    /// <summary>子を取り除く。</summary>
    /// <remarks>
    /// 子が削除された場合、所有者をLayout Dirtyとしてマークし、
    /// 次のパイプライン更新時に再レイアウトを要求します。
    /// 子が見つからなかった場合、Dirty状態は変更されません。
    /// </remarks>
    public bool Remove(T child)
    {
        if (!_children.Remove(child)) return false;

        owner.DropChild(child);
        return true;
    }

    /// <summary>全ての子を取り除く。</summary>
    /// <remarks>
    /// 子を一つ取り除くたびに所有者へLayout Dirtyを要求します。
    /// コレクションが空の場合、Dirty状態は変更されません。
    /// </remarks>
    public void Clear()
    {
        foreach (var child in _children)
        {
            owner.DropChild(child);
        }

        _children.Clear();
    }

    /// <summary>指定した子を保持しているかを判定する。</summary>
    /// <param name="item">検索する子。</param>
    /// <returns>指定した子を保持している場合は<c>true</c>、それ以外の場合は<c>false</c>。</returns>
    public bool Contains(T item) => _children.Contains(item);

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

    /// <summary>保持している子を列挙する反復子を取得する。</summary>
    /// <returns>子を列挙する反復子。</returns>
    public IEnumerator<T> GetEnumerator() => _children.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>
/// <see cref="MultiChildrenCollection{T}"/>向けの拡張メソッド。
/// </summary>
public static class MultiChildrenCollectionExtensions
{
    /// <summary>保持している子を指定した子の一覧で置き換える。</summary>
    /// <remarks>
    /// 既存の子の取り外しと新しい子の追加により、所有者をLayout Dirtyとしてマークします。
    /// 既存の子と指定した子がどちらも空の場合、Dirty状態は変更されません。
    /// </remarks>
    public static void Swap<T>(this MultiChildrenCollection<T> collection, ReadOnlySpan<T> children)
        where T : RenderObject
    {
        collection.Clear();

        foreach (var child in children)
        {
            collection.Add(child);
        }
    }

    /// <summary><see cref="RenderObject"/>を要素型へ変換して末尾に追加する。Element境界の非総称な挿入に使う。</summary>
    /// <remarks>
    /// コレクションの所有者をLayout Dirtyとしてマークし、
    /// 次のパイプライン更新時に再レイアウトを要求します。
    /// </remarks>
    public static void AddErased<T>(this MultiChildrenCollection<T> collection, RenderObject child)
        where T : RenderObject => collection.Add((T)child);

    /// <summary><see cref="RenderObject"/>を要素型として取り除く。要素型でない・保持していない場合は false。</summary>
    /// <remarks>
    /// 子が削除された場合、コレクションの所有者をLayout Dirtyとしてマークします。
    /// 型が一致しない場合、または子が見つからなかった場合、Dirty状態は変更されません。
    /// </remarks>
    public static bool RemoveErased<T>(this MultiChildrenCollection<T> collection, RenderObject child)
        where T : RenderObject => child is T typed && collection.Remove(typed);
}
