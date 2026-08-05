using System.Numerics;
using FloatSoda.Abstractions.Geometries;
using FloatSoda.Geometrics;
using FloatSoda.Gesture;
using FloatSoda.Rendering.Layers;
using SkiaSharp;

namespace FloatSoda.RenderObjects.Layout;

/// <summary>子のレイアウト寸法を含め、90度単位で回転するRenderObjectです。</summary>
public class RenderRotatedBox : RenderProxyBox
{
    /// <summary>時計回りの90度回転回数を0から3の範囲で取得または設定します。</summary>
    /// <remarks>設定値は4を法として正規化されます。</remarks>
    public int QuarterTurns
    {
        get;
        set
        {
            var normalized = Normalize(value);
            if (field == normalized) return;
            field = normalized;
            MarkNeedsLayout();
        }
    }

    /// <inheritdoc/>
    public override void PerformLayout()
    {
        if (Child is null)
        {
            Size = Constraints.Smallest;
            return;
        }

        var odd = QuarterTurns % 2 != 0;
        var childConstraints = odd
            ? new BoxConstraints(Constraints.MinHeight, Constraints.MaxHeight, Constraints.MinWidth, Constraints.MaxWidth)
            : Constraints;

        Child.Layout(childConstraints, parentUseSize: true);
        Size = odd
            ? Constraints.Constrain(Child.Size.Height, Child.Size.Width)
            : Constraints.Constrain(Child.Size);
    }

    /// <inheritdoc/>
    public override void Paint(PaintingContext context, Offset offset)
    {
        if (Child is null)
        {
            Layer = null;
            return;
        }

        var transform = GetTransform() * Matrix3x2.CreateTranslation((float)offset.X, (float)offset.Y);
        Layer = context.PushTransform(ToSkMatrix(transform), base.Paint, Layer as TransformLayer);
    }

    /// <inheritdoc/>
    public override bool HitTestChildren(HitTestResult result, Offset position)
    {
        if (!Matrix3x2.Invert(GetTransform(), out var inverse)) return false;
        var transformed = Vector2.Transform(new Vector2((float)position.X, (float)position.Y), inverse);
        return Child?.HitTest(result, new Offset(transformed.X, transformed.Y)) ?? false;
    }

    private Matrix3x2 GetTransform() => QuarterTurns switch
    {
        0 => Matrix3x2.Identity,
        1 => Matrix3x2.CreateRotation(MathF.PI / 2) * Matrix3x2.CreateTranslation(Size.Width, 0),
        2 => Matrix3x2.CreateRotation(MathF.PI) * Matrix3x2.CreateTranslation(Size.Width, Size.Height),
        3 => Matrix3x2.CreateRotation(3 * MathF.PI / 2) * Matrix3x2.CreateTranslation(0, Size.Height),
        _ => throw new InvalidOperationException("QuarterTurnsは0から3へ正規化されている必要があります。"),
    };

    private static int Normalize(int value) => ((value % 4) + 4) % 4;

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
}
