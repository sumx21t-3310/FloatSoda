using FloatSoda.Abstractions.Geometries;
using FloatSoda.Geometrics;
using SkiaSharp;

namespace FloatSoda.RenderObjects.Painting;

/// <summary>
/// 画像を自身の領域へ<see cref="Fit"/>に従って収めて描画し、その上に任意の子を描画するRenderObjectです。
/// </summary>
/// <remarks>
/// <see cref="Fit"/>の結果が自身の領域からはみ出す場合(<see cref="BoxFit.FitWidth"/>や
/// <see cref="BoxFit.Cover"/>など)、はみ出した部分は切り抜かれずそのまま描画されます。
/// 切り抜きが必要な場合は<c>ClipRect</c>で囲んでください。
/// </remarks>
public class RenderImage : RenderProxyBox
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
    public override void PerformLayout()
    {
        if (Child is not null)
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
    internal override SKSize ComputeDryLayout(Geometrics.BoxConstraints constraints) =>
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
        var imageSize = new SKSize(Image.Width, Image.Height);
        var fittedSizes = Fit.Apply(imageSize, Size);

        // Fitの結果が空になるのは、画像か自身の領域のどちらかが空のとき。
        // その場合はDrawImageに空の矩形を渡さず、子だけを描画する。
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

        if (Child != null) context.PaintChild(Child, offset);
    }

    private static void ValidateAlignment(Alignment value, string parameterName)
    {
        if (!float.IsFinite(value.X) || !float.IsFinite(value.Y))
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "配置値には有限値を指定してください。");
        }
    }
}
