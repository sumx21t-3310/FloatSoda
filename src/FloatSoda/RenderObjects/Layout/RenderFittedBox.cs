using System.Numerics;
using FloatSoda.Abstractions.Geometries;
using FloatSoda.Geometrics;
using FloatSoda.Gesture;
using FloatSoda.Rendering.Layers;
using SkiaSharp;

namespace FloatSoda.RenderObjects.Layout;

/// <summary>子を自然な大きさでレイアウトし、自身の領域へ拡大縮小して描画するRenderObjectです。</summary>
public sealed class RenderFittedBox : RenderProxyBox
{
    private bool _hasVisualOverflow;
    private Matrix3x2? _effectiveTransform;
    private TransformLayer? _transformLayer;

    /// <summary>子を自身の領域へ収める方法を取得または設定します。</summary>
    /// <exception cref="ArgumentOutOfRangeException">定義されていない値です。</exception>
    public BoxFit Fit
    {
        get;
        set
        {
            ValidateFit(value, nameof(Fit));
            if (field == value) return;

            var previous = field;
            field = value;
            ClearPaintData();
            if (AffectsLayout(previous) || AffectsLayout(value))
            {
                MarkNeedsLayout();
            }
            else
            {
                MarkNeedsPaint();
            }
        }
    } = BoxFit.Contain;

    /// <summary>拡大縮小後の子を自身の領域内へ配置する位置を取得または設定します。</summary>
    /// <exception cref="ArgumentOutOfRangeException">いずれかの成分が有限値ではありません。</exception>
    public Alignment Alignment
    {
        get;
        set
        {
            ValidateAlignment(value, nameof(Alignment));
            if (field == value) return;
            field = value;
            ClearPaintData();
            MarkNeedsPaint();
        }
    } = Alignment.Center;

    /// <summary>拡大縮小後の子が自身の領域からはみ出す場合の切り抜き方法を取得または設定します。</summary>
    /// <exception cref="ArgumentOutOfRangeException">定義されていない値です。</exception>
    public Clip ClipBehavior
    {
        get;
        set
        {
            ValidateClipBehavior(value, nameof(ClipBehavior));
            if (field == value) return;
            field = value;
            MarkNeedsPaint();
        }
    } = Clip.None;

    /// <inheritdoc/>
    public override void PerformLayout()
    {
        if (Child is null)
        {
            Size = Constraints.Smallest;
            ClearPaintData();
            return;
        }

        Child.Layout(BoxConstraints.Unbounded, parentUseSize: true);
        Size = ComputeFittedLayoutSize(Constraints, Child.Size, Fit);
        ClearPaintData();
    }

    /// <inheritdoc/>
    internal override SKSize ComputeDryLayout(BoxConstraints constraints)
    {
        if (Child is null) return constraints.Smallest;
        return ComputeFittedLayoutSize(constraints, Child.GetDryLayout(BoxConstraints.Unbounded), Fit);
    }

    /// <inheritdoc/>
    public override void Paint(PaintingContext context, Offset offset)
    {
        if (Child is null || Size.IsEmpty || Child.Size.IsEmpty)
        {
            Layer = null;
            return;
        }

        UpdatePaintData();
        if (_hasVisualOverflow && ClipBehavior != Clip.None)
        {
            Layer = context.PushClipRect(
                offset,
                SKRect.Create(Size),
                (clipContext, _) => PaintChildWithTransform(clipContext, offset),
                ClipBehavior,
                Layer as ClipRectLayer);
            return;
        }

        _transformLayer = context.PushTransform(
            ToSkMatrix(_effectiveTransform!.Value * Matrix3x2.CreateTranslation((float)offset.X, (float)offset.Y)),
            (transformContext, _) => transformContext.PaintChild(Child, Offset.Zero),
            _transformLayer);
        Layer = _transformLayer;
    }

    /// <inheritdoc/>
    public override bool HitTestChildren(HitTestResult result, Offset position)
    {
        if (Child is null || Size.IsEmpty || Child.Size.IsEmpty) return false;

        UpdatePaintData();
        if (!Matrix3x2.Invert(_effectiveTransform!.Value, out var inverse)) return false;

        var transformed = Vector2.Transform(new Vector2((float)position.X, (float)position.Y), inverse);
        return Child.HitTest(result, new Offset(transformed.X, transformed.Y));
    }

    private void PaintChildWithTransform(PaintingContext context, Offset offset)
    {
        _transformLayer = context.PushTransform(
            ToSkMatrix(_effectiveTransform!.Value * Matrix3x2.CreateTranslation((float)offset.X, (float)offset.Y)),
            (transformContext, _) => transformContext.PaintChild(Child!, Offset.Zero),
            _transformLayer);
    }

    private void UpdatePaintData()
    {
        if (_effectiveTransform is not null) return;

        if (Child is null)
        {
            _hasVisualOverflow = false;
            _effectiveTransform = Matrix3x2.Identity;
            return;
        }

        var fittedSizes = ApplyBoxFit(Fit, Child.Size, Size);
        if (fittedSizes.Source.IsEmpty || fittedSizes.Destination.IsEmpty)
        {
            _hasVisualOverflow = false;
            _effectiveTransform = Matrix3x2.Identity;
            return;
        }

        var sourceOffset = Alignment.ComputeOffset(Child.Size, fittedSizes.Source);
        var destinationOffset = Alignment.ComputeOffset(Size, fittedSizes.Destination);
        var scaleX = fittedSizes.Destination.Width / fittedSizes.Source.Width;
        var scaleY = fittedSizes.Destination.Height / fittedSizes.Source.Height;

        _hasVisualOverflow = fittedSizes.Source.Width < Child.Size.Width
                             || fittedSizes.Source.Height < Child.Size.Height;
        _effectiveTransform = Matrix3x2.CreateTranslation((float)-sourceOffset.X, (float)-sourceOffset.Y)
                              * Matrix3x2.CreateScale(scaleX, scaleY)
                              * Matrix3x2.CreateTranslation((float)destinationOffset.X, (float)destinationOffset.Y);
    }

    private void ClearPaintData()
    {
        _hasVisualOverflow = false;
        _effectiveTransform = null;
    }

    private static bool AffectsLayout(BoxFit fit) => fit == BoxFit.ScaleDown;

    private static SKSize ComputeFittedLayoutSize(BoxConstraints constraints, SKSize childSize, BoxFit fit)
    {
        var sizeConstraints = fit == BoxFit.ScaleDown ? constraints.Loosen : constraints;
        var unconstrainedSize = ConstrainPreservingAspectRatio(sizeConstraints, childSize);
        return constraints.Constrain(unconstrainedSize);
    }

    private static SKSize ConstrainPreservingAspectRatio(BoxConstraints constraints, SKSize size)
    {
        if (constraints.IsTight) return constraints.Smallest;
        if (size.IsEmpty) return constraints.Constrain(size);

        double width = size.Width;
        double height = size.Height;
        var aspectRatio = width / height;

        if (width > constraints.MaxWidth)
        {
            width = constraints.MaxWidth;
            height = width / aspectRatio;
        }

        if (height > constraints.MaxHeight)
        {
            height = constraints.MaxHeight;
            width = height * aspectRatio;
        }

        if (width < constraints.MinWidth)
        {
            width = constraints.MinWidth;
            height = width / aspectRatio;
        }

        if (height < constraints.MinHeight)
        {
            height = constraints.MinHeight;
            width = height * aspectRatio;
        }

        return constraints.Constrain(width, height);
    }

    private static FittedSizes ApplyBoxFit(BoxFit fit, SKSize input, SKSize output)
    {
        if (input.IsEmpty || output.IsEmpty) return new FittedSizes(SKSize.Empty, SKSize.Empty);

        var inputAspectRatio = input.Width / input.Height;
        var outputAspectRatio = output.Width / output.Height;

        return fit switch
        {
            BoxFit.Fill => new FittedSizes(input, output),
            BoxFit.Contain => outputAspectRatio > inputAspectRatio
                ? new FittedSizes(input, new SKSize(input.Width * output.Height / input.Height, output.Height))
                : new FittedSizes(input, new SKSize(output.Width, input.Height * output.Width / input.Width)),
            BoxFit.Cover => outputAspectRatio > inputAspectRatio
                ? new FittedSizes(new SKSize(input.Width, input.Width * output.Height / output.Width), output)
                : new FittedSizes(new SKSize(input.Height * output.Width / output.Height, input.Height), output),
            BoxFit.FitWidth => outputAspectRatio > inputAspectRatio
                ? new FittedSizes(new SKSize(input.Width, input.Width * output.Height / output.Width), output)
                : new FittedSizes(input, new SKSize(output.Width, input.Height * output.Width / input.Width)),
            BoxFit.FitHeight => outputAspectRatio > inputAspectRatio
                ? new FittedSizes(input, new SKSize(input.Width * output.Height / input.Height, output.Height))
                : new FittedSizes(new SKSize(input.Height * output.Width / output.Height, input.Height), output),
            BoxFit.None => new FittedSizes(
                new SKSize(Math.Min(input.Width, output.Width), Math.Min(input.Height, output.Height)),
                new SKSize(Math.Min(input.Width, output.Width), Math.Min(input.Height, output.Height))),
            BoxFit.ScaleDown => ScaleDown(input, output),
            _ => throw new ArgumentOutOfRangeException(nameof(fit), fit, "定義済みのBoxFitを指定してください。"),
        };
    }

    private static FittedSizes ScaleDown(SKSize input, SKSize output)
    {
        var destination = input;
        var aspectRatio = input.Width / input.Height;
        if (destination.Height > output.Height)
        {
            destination = new SKSize(output.Height * aspectRatio, output.Height);
        }

        if (destination.Width > output.Width)
        {
            destination = new SKSize(output.Width, output.Width / aspectRatio);
        }

        return new FittedSizes(input, destination);
    }

    private static SKMatrix ToSkMatrix(Matrix3x2 matrix) => new()
    {
        ScaleX = matrix.M11,
        SkewX = matrix.M21,
        TransX = matrix.M31,
        SkewY = matrix.M12,
        ScaleY = matrix.M22,
        TransY = matrix.M32,
        Persp0 = 0,
        Persp1 = 0,
        Persp2 = 1,
    };

    internal static void ValidateFit(BoxFit value, string parameterName)
    {
        if (!Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "定義済みのBoxFitを指定してください。");
        }
    }

    internal static void ValidateAlignment(Alignment value, string parameterName)
    {
        if (!float.IsFinite(value.X) || !float.IsFinite(value.Y))
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "配置値には有限値を指定してください。");
        }
    }

    internal static void ValidateClipBehavior(Clip value, string parameterName)
    {
        if (!Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "定義済みのクリップ方法を指定してください。");
        }
    }

    private readonly record struct FittedSizes(SKSize Source, SKSize Destination);
}
