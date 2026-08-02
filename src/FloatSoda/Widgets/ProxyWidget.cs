using FloatSoda.Elements;

namespace FloatSoda.Widgets;

/// <summary>
/// 単一の子Widgetへ構成を委譲し、自身ではRenderObjectを生成しないWidgetの基底型です。
/// </summary>
/// <seealso cref="ProxyElement"/>
public abstract record ProxyWidget : Widget
{
    /// <summary>
    /// このWidgetの直下に配置する子Widgetを取得します。
    /// </summary>
    public required Widget Child { get; init; }
}
