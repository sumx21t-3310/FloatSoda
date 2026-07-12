using FloatSoda.Animation;
using FloatSoda.Common.Geometries;
using FloatSoda.Common.Layer;
using FloatSoda.Core;

namespace FloatSoda.RenderObjects.Animation;

/// <summary>
/// アニメーションで駆動される不透明度を子に適用するRenderObject。
/// <see cref="IAnimation{T}.Changed"/>を購読し、値が変わったフレームだけ再ペイントする
/// (Widgetのリビルドは発生しない)。FlutterのRenderAnimatedOpacity相当。
/// </summary>
public class RenderAnimatedOpacity : RenderProxyBox
{
    private byte? Alpha = null;

    /// <summary>不透明度を駆動するアニメーション(0.0〜1.0)。</summary>
    public required IAnimation<double> Opacity
    {
        get;
        set
        {
            if (ReferenceEquals(field, value)) return;

            if (Attached && field != null)
            {
                field.Changed -= UpdateOpacity;
            }

            field = value;

            if (Attached)
            {
                value.Changed += UpdateOpacity;
                UpdateOpacity();
            }
        }
    }

    public override void Attach(RenderPipeline? owner)
    {
        base.Attach(owner);

        Opacity.Changed += UpdateOpacity;
        UpdateOpacity();
    }

    public override void Detach()
    {
        Opacity.Changed -= UpdateOpacity;

        base.Detach();
    }

    private void UpdateOpacity()
    {
        var oldAlpha = Alpha;

        Alpha = (byte)double.Round(Opacity.Value * 255);

        if (oldAlpha != Alpha) MarkNeedsPaint();
    }

    public override void Paint(PaintingContext context, Offset offset)
    {
        if (Child is null) return;

        Layer = Alpha switch
        {
            0 => null,
            _ => context.PushOpacity(offset, Alpha ?? 0, base.Paint, Layer as OpacityLayer)
        };
    }
}
