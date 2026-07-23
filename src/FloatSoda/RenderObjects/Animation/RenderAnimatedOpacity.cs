using FloatSoda.Animation;
using FloatSoda.Abstractions.Geometries;
using FloatSoda.Rendering.Layers;
using FloatSoda.Core;

namespace FloatSoda.RenderObjects.Animation;

/// <summary>
/// アニメーションで駆動される不透明度を子に適用するRenderObject。
/// <see cref="IListenable.Changed"/>を購読し、値が変わったフレームだけ再ペイントする
/// (Widgetのリビルドは発生しない)。FlutterのRenderAnimatedOpacity相当。
/// </summary>
public class RenderAnimatedOpacity : RenderProxyBox
{
    private byte? Alpha = null;

    /// <summary>不透明度を駆動するアニメーションを取得または設定します。</summary>
    /// <remarks>
    /// 値は0.0から1.0の範囲で指定します。
    /// アタッチ中に参照が変更された場合は以前のアニメーションの購読を解除し、
    /// 新しいアニメーションを購読して現在値を反映します。
    /// 反映後のアルファ値が変化した場合、このRenderObjectをPaint Dirtyとしてマークし、
    /// 次のパイプライン更新時に不透明度レイヤーと子を再描画します。
    /// 同じ参照が設定された場合、購読とDirty状態は変更されません。
    /// </remarks>
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

    /// <inheritdoc/>
    public override void Attach(RenderPipeline? owner)
    {
        base.Attach(owner);

        Opacity.Changed += UpdateOpacity;
        UpdateOpacity();
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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
