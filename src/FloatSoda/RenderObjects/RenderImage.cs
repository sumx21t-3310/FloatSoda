using FloatSoda.Abstractions.Geometries;
using SkiaSharp;

namespace FloatSoda.RenderObjects;

/// <summary>
/// 画像を自身の領域へ拡縮して描画し、その上に任意の子を描画するRenderObjectです。
/// </summary>
public class RenderImage : RenderProxyBox
{
    /// <summary>
    /// 描画する画像を取得します。
    /// </summary>
    public required SKImage Image { get; init; }

    /// <inheritdoc/>
    public override void PerformLayout()
    {
        if (Child != null)
        {
            Child.Layout(Constraints);
            Size = Child.Size;
        }
        else
        {
            Size = Constraints.Constrain(new SKSize(Image.Width, Image.Height));
        }
    }

    /// <inheritdoc/>
    internal override SKSize ComputeDryLayout(FloatSoda.Geometrics.BoxConstraints constraints) =>
        Child?.GetDryLayout(constraints) ?? constraints.Constrain(Image.Width, Image.Height);

    /// <inheritdoc/>
    protected override double ComputeMinIntrinsicWidth(double height) => Child?.GetMinIntrinsicWidth(height) ?? Image.Width;

    /// <inheritdoc/>
    protected override double ComputeMaxIntrinsicWidth(double height) => Child?.GetMaxIntrinsicWidth(height) ?? Image.Width;

    /// <inheritdoc/>
    protected override double ComputeMinIntrinsicHeight(double width) => Child?.GetMinIntrinsicHeight(width) ?? Image.Height;

    /// <inheritdoc/>
    protected override double ComputeMaxIntrinsicHeight(double width) => Child?.GetMaxIntrinsicHeight(width) ?? Image.Height;

    /// <inheritdoc/>
    public override void Paint(PaintingContext context, Offset offset)
    {
        context.Canvas.DrawImage(Image, SKRect.Create(offset, Size));
        if (Child != null) context.PaintChild(Child, offset);
    }
}
