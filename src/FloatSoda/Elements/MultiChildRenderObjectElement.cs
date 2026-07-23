using FloatSoda.RenderObjects;
using FloatSoda.Widgets;

namespace FloatSoda.Elements;

/// <summary>
/// 複数の子Widgetを持つRenderObjectWidgetを、複数の子RenderObjectへ対応付けるElementです。
/// </summary>
/// <typeparam name="T">このElementが管理するRenderObjectの型。</typeparam>
/// <seealso cref="MultiChildRenderObjectWidget{T}"/>
public class MultiChildRenderObjectElement<T> : RenderObjectElement<T> where T : RenderObject
{
    /// <summary>
    /// このElementが現在管理している複数子用Widgetを取得します。
    /// </summary>
    public MultiChildRenderObjectWidget<T> WidgetCasted => (MultiChildRenderObjectWidget<T>)Widget!;

    private List<Element> Children { get; set; } = [];

    /// <inheritdoc/>
    public override RenderObject? RenderObject { get; protected set; }

    /// <summary>
    /// ElementツリーとRenderObjectツリーへ接続し、Widgetが宣言するすべての子Elementを生成します。
    /// </summary>
    /// <param name="parent">接続先の親Element。ルートとして接続する場合は<see langword="null"/>。</param>
    public override void Mount(Element? parent)
    {
        base.Mount(parent);
        Children = WidgetCasted.Children.Select(InflateWidget).ToList();
    }

    /// <summary>
    /// 管理するWidgetを置き換え、キーとWidget型に基づいて既存の子Elementを再利用しながら子一覧を更新します。
    /// </summary>
    /// <param name="newWidget">同じRenderObjectを引き継ぐ新しい複数子用Widget。</param>
    public override void Update(Widget newWidget)
    {
        base.Update(newWidget);
        Children = UpdateChildren(Children, WidgetCasted.Children);
    }

    /// <inheritdoc/>
    public override void VisitChildren(Action<Element> visitor) => Children.ForEach(visitor);

    /// <inheritdoc/>
    public override void InsertRenderObjectChild(RenderObject child)
    {
        if (RenderObject is IHasMultiChildrenRenderObject ro) ro.AddChild(child);
    }

    /// <inheritdoc/>
    public override void RemoveRenderObjectChild(RenderObject? child)
    {
        if (child != null && RenderObject is IHasMultiChildrenRenderObject ro) ro.RemoveChild(child);
    }
}