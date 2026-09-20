using FloatSoda.Abstractions.Geometries;
using FloatSoda.Geometrics;
using SkiaSharp;

namespace FloatSoda.RenderObjects.Painting;

/// <summary>
/// 画像を自身の領域へ<see cref="Fit"/>に従って収めて描画する、子を持たないRenderObjectです。
/// </summary>
/// <remarks>
/// <see cref="BoxFit.Cover"/>のように画像の一部だけを使う<see cref="Fit"/>では、描画元の矩形を
/// 切り取って描画します。どの<see cref="Fit"/>でも描画先は自身の領域内に収まるため、
/// 領域外へはみ出すことはありません。
/// <see cref="FloatSoda.Widgets.Layout.FittedBox"/>が<c>ClipBehavior</c>を必要とするのは、
/// 描画元を切り取れない子ウィジェットを拡大縮小するためで、この型では不要です。
/// 制約が許す範囲で、画像の原寸を自身のサイズにします。
/// </remarks>
public class RenderImage : RenderBox
{
    /// <summary>
    /// 描画する画像を取得します。
    /// </summary>
    public required SKImage Image
    {
        get;
        set
        {
            if (ReferenceEquals(field, value)) return;
            field = value;
            MarkNeedsLayout();
        }
    }

    /// <summary>画像を自身の領域へ収める方法を取得または設定します。</summary>
    /// <remarks>レイアウト結果は変えません。変更すると再描画のみを要求します。</remarks>
    /// <exception cref="ArgumentOutOfRangeException">定義されていない値です。</exception>
    public BoxFit Fit
    {
        get;
        set
        {
            BoxFitExtensions.Validate(value, nameof(Fit));
            if (field == value) return;
            field = value;
            MarkNeedsPaint();
        }
    } = BoxFit.Contain;

    /// <summary>収めた画像を自身の領域内へ配置する位置を取得または設定します。</summary>
    /// <remarks>レイアウト結果は変えません。変更すると再描画のみを要求します。</remarks>
    /// <exception cref="ArgumentOutOfRangeException">いずれかの成分が有限値ではありません。</exception>
    public Alignment Alignment
    {
        get;
        set
        {
            ValidateAlignment(value, nameof(Alignment));
            if (field == value) return;
            field = value;
            MarkNeedsPaint();
        }
    } = Alignment.Center;

    /// <inheritdoc/>
    public override void PerformLayout() => Size = Constraints.Constrain(new SKSize(Image.Width, Image.Height));

    /// <inheritdoc/>
    internal override SKSize ComputeDryLayout(Geometrics.BoxConstraints constraints) =>
        constraints.Constrain(Image.Width, Image.Height);

    /// <inheritdoc/>
    protected override double ComputeMinIntrinsicWidth(double height) => Image.Width;

    /// <inheritdoc/>
    protected override double ComputeMaxIntrinsicWidth(double height) => Image.Width;

    /// <inheritdoc/>
    protected override double ComputeMinIntrinsicHeight(double width) => Image.Height;

    /// <inheritdoc/>
    protected override double ComputeMaxIntrinsicHeight(double width) => Image.Height;

    /// <inheritdoc/>
    public override void Paint(PaintingContext context, Offset offset)
    {
        var imageSize = new SKSize(Image.Width, Image.Height);
        var fittedSizes = Fit.Apply(imageSize, Size);

        // Fitの結果が空になるのは、画像か自身の領域のどちらかが空のとき。
        // その場合はDrawImageに空の矩形を渡さず、何も描画しない。
        if (!fittedSizes.Source.IsEmpty && !fittedSizes.Destination.IsEmpty)
        {
            var sourceOffset = Alignment.ComputeOffset(imageSize, fittedSizes.Source);
            var destinationOffset = Alignment.ComputeOffset(Size, fittedSizes.Destination);

            var source = SKRect.Create(
                (float)sourceOffset.X,
                (float)sourceOffset.Y,
                fittedSizes.Source.Width,
                fittedSizes.Source.Height);
            var destination = SKRect.Create(
                (float)(offset.X + destinationOffset.X),
                (float)(offset.Y + destinationOffset.Y),
                fittedSizes.Destination.Width,
                fittedSizes.Destination.Height);

            context.Canvas.DrawImage(Image, source, destination);
        }
    }

    private static void ValidateAlignment(Alignment value, string parameterName)
    {
        if (!float.IsFinite(value.X) || !float.IsFinite(value.Y))
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "配置値には有限値を指定してください。");
        }
    }
}
