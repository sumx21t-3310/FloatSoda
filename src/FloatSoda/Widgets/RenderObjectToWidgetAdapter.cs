using FloatSoda.Elements;
using FloatSoda.RenderObjects;

namespace FloatSoda.Widgets;

/// <summary>
/// 既存の<see cref="RenderView"/>をルートとして、WidgetツリーとRenderObjectツリーを接続します。
/// </summary>
/// <remarks>
/// 初回接続ではルートElementをマウントし、再接続では同じElementを再利用して新しい子構成の再ビルドを予約します。
/// </remarks>
public record RenderObjectToWidgetAdapter : RenderObjectWidget<RenderView>
{
    /// <summary>
    /// Widgetツリーの描画先となる既存のルートRenderViewを取得します。
    /// このアダプターは新しいRenderViewを生成せず、指定されたインスタンスを使用します。
    /// </summary>
    public required RenderView Container { get; init; }
    /// <summary>
    /// ルートRenderViewの子として構築するウィジェットを取得します。
    /// <see langword="null"/>の場合、ルートは子を持ちません。
    /// </summary>
    public Widget? Child { get; init; }
    /// <inheritdoc/>
    public override Element CreateElement() => new RenderObjectToWidgetElement<RenderView>();

    /// <summary>
    /// <see cref="Container"/>で指定された既存のRenderViewを返します。
    /// </summary>
    /// <returns>Widgetツリーのルートとして使用するRenderView。</returns>
    public override RenderView CreateRenderObject() => Container;

    /// <summary>
    /// このアダプターをWidgetツリーのルートへ接続または再接続します。
    /// </summary>
    /// <param name="owner">ルートElementの再ビルドを管理するBuildOwner。</param>
    /// <param name="element">
    /// 再利用する既存のルートElement。初回接続では<see langword="null"/>を指定します。
    /// </param>
    /// <returns>
    /// 初回接続で生成された、または再接続で再利用されたルートElement。
    /// </returns>
    /// <remarks>
    /// <paramref name="element"/>が<see langword="null"/>の場合は、BuildOwnerのビルドスコープ内で
    /// 新しいElementをマウントします。既存のElementを指定した場合は、このアダプターを次の構成として保持し、
    /// Elementを再ビルド対象としてスケジュールします。
    /// </remarks>
    public RenderObjectToWidgetElement<RenderView> AttachToRenderTree(BuildOwner owner, RenderObjectToWidgetElement<RenderView>? element)
    {
        RenderObjectToWidgetElement<RenderView> result;
        if (element == null)
        {
            result = (RenderObjectToWidgetElement<RenderView>)CreateElement();
            result.Widget = this;
            result.Owner = owner;
            owner.BuildScope(() => result.Mount(null));
        }
        else
        {
            result = element;
            result.NewWidget = this;
            result.MarkNeedsBuild();
        }

        return result;
    }
}