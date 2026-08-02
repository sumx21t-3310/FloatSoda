namespace FloatSoda.RenderObjects.Painting;

/// <summary>
/// 子サブツリーの描画を独立した合成レイヤーへ記録する再描画境界です。
/// </summary>
public class RenderRepaintBoundary : RenderProxyBox
{
    /// <inheritdoc/>
    public override bool IsRepaintBoundary => true;
}
