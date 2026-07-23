using FloatSoda.RenderObjects;
using FloatSoda.Widgets;

namespace FloatSoda.Elements;

/// <summary>
/// Widgetツリー上の位置と可変状態を保持し、Widgetの更新をRenderObjectツリーへ反映する要素の基底クラスです。
/// </summary>
/// <seealso cref="Widget"/>
/// <seealso cref="RenderObject"/>
public abstract class Element : IBuildContext, IComparable<Element>
{
    /// <summary>
    /// このElementが現在管理しているWidgetを取得または設定します。
    /// ElementへWidgetが割り当てられる前は<see langword="null"/>です。
    /// </summary>
    public Widget? Widget { get; set; }

    /// <summary>
    /// Elementツリー上の親を取得または設定します。
    /// ルートまたはツリーから切り離されたElementでは<see langword="null"/>です。
    /// </summary>
    public Element? Parent { get; set; }

    /// <summary>
    /// Elementツリー上の深さを取得または設定します。
    /// ルートを1とし、子へ進むごとに1ずつ増加します。
    /// </summary>
    public int Depth { get; set; }

    /// <summary>
    /// このElement以下で最初に見つかるRenderObjectを取得します。
    /// 対応するRenderObjectが存在しない場合は<see langword="null"/>です。
    /// </summary>
    /// <seealso cref="RenderObjectElement"/>
    public virtual RenderObject? RenderObject
    {
        get
        {
            RenderObject? result = null;

            Visit(this);

            return result;

            void Visit(Element element)
            {
                if (element is RenderObjectElement)
                {
                    result = element.RenderObject;
                }
                else
                {
                    element.VisitChildren(Visit);
                }
            }
        }
        protected set => field = value;
    }

    /// <summary>
    /// このElementから参照できるInheritedWidgetの登録型と、最近傍の対応Elementとのマップを取得または設定します。
    /// 継承情報がまだ構成されていない場合は<see langword="null"/>です。
    /// </summary>
    public Dictionary<Type, InheritedElement>? InheritedWidgets { get; set; }

    /// <summary>
    /// このElementが依存関係を登録したInheritedElementの集合を取得します。
    /// </summary>
    protected HashSet<InheritedElement> Dependencies { get; } = [];

    /// <summary>
    /// このElementツリーの再構築を管理する所有者を取得または設定します。
    /// ツリーへ接続されていない場合は<see langword="null"/>です。
    /// </summary>
    public BuildOwner? Owner { get; set; }

    private InheritedWidget DependOnInheritedElement(InheritedElement ancestor)
    {
        Dependencies.Add(ancestor);
        ancestor.UpdateDependencies(this);
        return ancestor.Widget as InheritedWidget;
    }

    /// <inheritdoc/>
    public T? DependOnInheritedWidgetOfExactType<T>() where T : InheritedWidget
    {
        if (InheritedWidgets?.TryGetValue(typeof(T), out var ancestor) ?? false)
        {
            return DependOnInheritedElement(ancestor) as T;
        }

        return null;
    }

    /// <inheritdoc/>
    public virtual InheritedElement? GetElementForInheritedWidgetOfExactType<T>() where T : InheritedWidget
    {
        if (InheritedWidgets?.TryGetValue(typeof(T), out var inheritedElement) ?? false)
        {
            return inheritedElement;
        }

        return null;
    }

    /// <summary>
    /// 親Elementから、このElementで参照可能なInheritedWidgetのマップを引き継ぎます。
    /// </summary>
    protected virtual void UpdateInheritance() => InheritedWidgets = Parent?.InheritedWidgets;

    /// <summary>
    /// このElementが<see cref="BuildOwner"/>の再構築待ちリストに登録されているかどうかを取得または設定します。
    /// </summary>
    /// <remarks>この値を直接変更しても、再構築は予約されません。</remarks>
    public bool InDirtyList { get; set; }
    /// <summary>
    /// このElementが再構築を必要としているかどうかを取得または設定します。
    /// </summary>
    /// <remarks>再構築を予約する場合は、この値を直接設定せず<see cref="MarkNeedsBuild"/>を使用します。</remarks>
    public bool Dirty { get; set; }

    /// <summary>
    /// このElementが直接管理する子Elementを列挙します。
    /// </summary>
    /// <param name="visitor">各子Elementに一度ずつ適用する処理。</param>
    public virtual void VisitChildren(Action<Element> visitor) { }

    /// <summary>
    /// このElementを指定した親の子としてElementツリーへ接続します。
    /// </summary>
    /// <param name="parent">接続先の親Element。ルートとして接続する場合は<see langword="null"/>。</param>
    /// <remarks>
    /// 親から所有者と継承情報を引き継ぎ、深さを設定します。
    /// このメソッドだけではWidgetの再構築やRenderObjectの接続は行いません。
    /// </remarks>
    public virtual void Mount(Element? parent)
    {
        Parent = parent;
        Depth = parent != null ? parent.Depth + 1 : 1;

        if (Parent != null)
        {
            Owner = Parent.Owner;
        }

        UpdateInheritance();
    }


    /// <summary>
    /// 既存の子Elementを新しいWidget構成へ更新し、再利用できない場合は子Elementを置き換えます。
    /// </summary>
    /// <param name="child">現在の子Element。子が存在しない場合は<see langword="null"/>。</param>
    /// <param name="newWidget">子として適用するWidget。子を取り除く場合は<see langword="null"/>。</param>
    /// <returns>更新または生成された子Element。子を取り除いた場合は<see langword="null"/>。</returns>
    protected Element? UpdateChild(Element? child, Widget? newWidget)
    {
        if (newWidget == null)
        {
            if (child != null)
            {
                DeactivateChild(child);
            }

            return null;
        }


        if (child == null) return InflateWidget(newWidget);


        if (child.Widget == newWidget)
        {
            return child;
        }

        if (Widget.CanUpdate(child.Widget, newWidget))
        {
            child.Update(newWidget);
            return child;
        }

        DeactivateChild(child);
        return InflateWidget(newWidget);
    }

    /// <summary>
    /// このElementが管理するWidgetを、更新後の構成へ置き換えます。
    /// </summary>
    /// <param name="newWidget">このElementと同じ位置を引き継ぐ新しいWidget。</param>
    /// <remarks>基底実装はWidgetの置き換えだけを行い、再構築を予約しません。</remarks>
    public virtual void Update(Widget newWidget) => Widget = newWidget;


    /// <summary>
    /// 指定したWidgetに対応するElementを生成し、このElementの子としてマウントします。
    /// </summary>
    /// <param name="newWidget">Elementを生成するWidget。</param>
    /// <returns>生成してマウントされた子Element。</returns>
    protected Element InflateWidget(Widget newWidget)
    {
        var newChild = newWidget.CreateElement();

        newChild.Widget = newWidget;
        newChild.Mount(this);

        return newChild;
    }

    /// <summary>
    /// このElement以下のRenderObjectを、祖先側のRenderObjectツリーへ接続します。
    /// </summary>
    public virtual void AttachRenderObject() { }

    /// <summary>
    /// このElement以下のRenderObjectをRenderObjectツリーから切り離します。
    /// </summary>
    public virtual void DetachRenderObject()
    {
        VisitChildren(child => child.DetachRenderObject());
    }

    /// <summary>
    /// 指定した子ElementをElementツリーとRenderObjectツリーから切り離し、依存関係を解除します。
    /// </summary>
    /// <param name="child">切り離す直接の子Element。</param>
    protected void DeactivateChild(Element child)
    {
        child.Parent = null;
        child.DetachRenderObject();
        child.Deactivate();
    }

    /// <summary>
    /// このElement以下のサブツリーをツリーから切り離す際に、依存していた
    /// <see cref="InheritedElement"/> の被依存リストから自身を除去する。
    /// これを行わないと、破棄済みElementへ<see cref="DidChangeDependencies"/>が
    /// 呼ばれ続け、リークの原因になる。
    /// </summary>
    protected virtual void Deactivate()
    {
        foreach (var dependency in Dependencies)
        {
            dependency.RemoveDependent(this);
        }

        Dependencies.Clear();

        VisitChildren(child => child.Deactivate());
    }

    /// <summary>
    /// このElementが依存するInheritedWidgetの値が変更されたことを通知します。
    /// </summary>
    /// <remarks>基底実装はこのElementの再構築を要求します。</remarks>
    public virtual void DidChangeDependencies() => MarkNeedsBuild();

    /// <summary>
    /// このElementを次のビルド処理で再構築するよう要求します。
    /// </summary>
    /// <remarks>
    /// Elementを再構築が必要な状態にし、所有者の再構築待ちリストへ登録します。
    /// すでに再構築が必要な場合は、状態と予約を変更しません。
    /// </remarks>
    public void MarkNeedsBuild()
    {
        if (Dirty) return;

        Dirty = true;
        Owner?.ScheduledBuildFor(this);
    }

    /// <summary>
    /// ホットリロード時に呼ばれ、このElement以下のサブツリー全体を再ビルド対象にする。
    /// Widgetのrecord等価による差分スキップに関わらず、全ComponentElementのBuild()が再実行される。
    /// </summary>
    public void Reassemble()
    {
        MarkNeedsBuild();
        VisitChildren(child => child.Reassemble());
    }

    /// <summary>
    /// このElement固有の再構築処理を直ちに実行します。
    /// </summary>
    /// <remarks>通常のフレーム更新では<see cref="BuildOwner.BuildScope"/>から呼び出されます。</remarks>
    public void Rebuild() => PerformRebuild();

    /// <summary>
    /// 派生Elementが担当する再構築処理を実行します。
    /// </summary>
    public abstract void PerformRebuild();

    /// <summary>
    /// Elementの再構築順序を、ツリーの深さと再構築要否に基づいて比較します。
    /// </summary>
    /// <param name="other">比較対象のElement。<see langword="null"/>は指定できません。</param>
    /// <returns>
    /// このElementが先なら負の値、同順位なら0、後なら正の値。
    /// </returns>
    public int CompareTo(Element? other)
    {
        if (Depth < other.Depth) return -1;
        if (Depth > other.Depth) return 1;
        if (!Dirty && other.Dirty) return -1;
        if (Dirty && !other.Dirty) return 1;
        return 0;
    }
}