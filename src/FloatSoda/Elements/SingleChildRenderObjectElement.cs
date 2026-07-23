using FloatSoda.RenderObjects;
using FloatSoda.Widgets;

namespace FloatSoda.Elements;

/// <summary>
/// 単一の子Widgetを持つRenderObjectWidgetを、単一の子RenderObjectへ対応付けるElementです。
/// </summary>
/// <typeparam name="T">このElementが管理するRenderObjectの型。</typeparam>
/// <seealso cref="SingleChildRenderObjectWidget{T}"/>
public class SingleChildRenderObjectElement<T> : RenderObjectElement<T> where T : RenderObject
{
    /// <summary>
    /// このElementが現在管理している単一子用Widgetを取得します。
    /// Widgetが割り当てられていない場合、または型が一致しない場合は<see langword="null"/>です。
    /// </summary>
    public SingleChildRenderObjectWidget<T>? WidgetCasted => Widget as SingleChildRenderObjectWidget<T>;

    private Element? Child { get; set; }

    /// <inheritdoc/>
    public override RenderObject? RenderObject { get; protected set; }

    /// <summary>
    /// ElementツリーとRenderObjectツリーへ接続し、Widgetが宣言する子Elementを生成します。
    /// </summary>
    /// <param name="parent">接続先の親Element。ルートとして接続する場合は<see langword="null"/>。</param>
    public override void Mount(Element? parent)
    {
        base.Mount(parent);
        Child = UpdateChild(Child, WidgetCasted?.Child);
    }


    /// <summary>
    /// 管理するWidgetを置き換え、Widget型とキーが一致する場合は既存の子Elementを再利用して子を更新します。
    /// </summary>
    /// <param name="newWidget">同じRenderObjectを引き継ぐ新しい単一子用Widget。</param>
    public override void Update(Widget newWidget)
    {
        base.Update(newWidget);
        Child = UpdateChild(Child, WidgetCasted?.Child);
    }

    /// <inheritdoc/>
    public override void InsertRenderObjectChild(RenderObject? child)
    {
        if (RenderObject is IHasSingleChildRenderObject ro) ro.Child = child;
    }

    /// <inheritdoc/>
    public override void RemoveRenderObjectChild(RenderObject? child)
    {
        if (RenderObject is IHasSingleChildRenderObject ro) ro.Child = null;
    }

    /// <inheritdoc/>
    public override void VisitChildren(Action<Element> visitor)
    {
        if (Child != null) visitor(Child);
    }
}