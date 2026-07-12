using FloatSoda.Animation;
using FloatSoda.RenderObjects.Animation;

namespace FloatSoda.Widgets.Animation;

/// <summary>
/// アニメーションで子の不透明度を駆動するウィジェット。FlutterのFadeTransition相当。
/// 値の反映は<see cref="IAnimation{T}.Changed"/>購読によるペイントのみで行われるため、
/// フレームごとのリビルド(SetState)は不要です。
/// </summary>
public record FadeTransition : SingleChildRenderObjectWidget<RenderAnimatedOpacity>
{
    /// <summary>不透明度を駆動するアニメーション(0.0〜1.0)。</summary>
    public required IAnimation<double> Opacity { get; init; }

    public override RenderAnimatedOpacity CreateRenderObject() => new()
    {
        Opacity = Opacity
    };

    public override void UpdateRenderObject(RenderAnimatedOpacity renderObject) => renderObject.Opacity = Opacity;
}
