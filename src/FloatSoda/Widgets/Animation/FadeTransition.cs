using FloatSoda.Animation;
using FloatSoda.RenderObjects.Animation;

namespace FloatSoda.Widgets.Animation;

/// <summary>
/// アニメーションで子の不透明度を駆動するウィジェット。FlutterのFadeTransition相当。
/// 値の反映は<see cref="IAnimation{T}.Changed"/>購読によるペイントのみで行われるため、
/// フレームごとのリビルド(SetState)は不要です。
/// </summary>
/// <seealso cref="RenderAnimatedOpacity"/>
public record FadeTransition : SingleChildRenderObjectWidget<RenderAnimatedOpacity>
{
    /// <summary>不透明度を駆動するアニメーション(0.0〜1.0)。</summary>
    public required IAnimation<double> Opacity { get; init; }

    /// <summary>
    /// 不透明度アニメーションを監視して子要素へ適用するRenderObjectを生成します。
    /// </summary>
    /// <returns>指定されたアニメーションを保持する新しいRenderObject。</returns>
    public override RenderAnimatedOpacity CreateRenderObject() => new()
    {
        Opacity = Opacity
    };

    /// <summary>
    /// 不透明度を駆動するアニメーションを既存のRenderObjectへ反映します。
    /// </summary>
    /// <param name="renderObject">このウィジェットに対応するRenderObject。</param>
    /// <remarks>
    /// アニメーションが置き換えられた場合、以前の変更通知の購読を解除して新しい通知を購読します。
    /// 現在の不透明度が変化した場合は対象をPaint Dirtyとしてマークし、次のパイプライン更新時に再描画します。
    /// 同じアニメーションが指定された場合、購読とDirty状態は変更されません。
    /// </remarks>
    public override void UpdateRenderObject(RenderAnimatedOpacity renderObject) => renderObject.Opacity = Opacity;
}
