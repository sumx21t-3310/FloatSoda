using FloatSoda.Widgets;

namespace FloatSoda.Elements;

/// <summary>
/// <see cref="ProxyWidget"/>の単一の子WidgetをElementツリーへ反映するElementの基底型です。
/// </summary>
/// <seealso cref="ProxyWidget"/>
public abstract class ProxyElement : ComponentElement
{
    /// <inheritdoc/>
    public override Widget Build() => ((ProxyWidget)Widget!).Child;

    /// <summary>
    /// 管理するProxyWidgetを置き換え、既存の子Elementを可能な限り再利用して直ちに更新します。
    /// </summary>
    /// <param name="newWidget">同じ位置を引き継ぐ新しいProxyWidget。</param>
    public override void Update(Widget newWidget)
    {
        base.Update(newWidget);
        Dirty = true;
        Rebuild();
    }
}
