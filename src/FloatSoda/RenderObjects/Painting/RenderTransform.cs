using System.Numerics;
using FloatSoda.Abstractions.Geometries;
using FloatSoda.Geometrics;
using FloatSoda.Gesture;
using FloatSoda.Rendering.Layers;
using SkiaSharp;

namespace FloatSoda.RenderObjects.Painting;

/// <summary>
/// レイアウト後の子へ2次元変換を適用するRenderObjectです。
/// </summary>
public class RenderTransform : RenderProxyBox
{
    /// <summary>
    /// 子へ適用する2次元アフィン変換を取得または設定します。
    /// </summary>
    public Matrix3x2 Transform
    {
        get;
        set
        {
            ValidateMatrix(value);
            if (field == value) return;
            field = value;
            MarkNeedsPaint();
        }
    } = Matrix3x2.Identity;

    /// <summary>
    /// 変換原点へ加算するローカル座標のオフセットを取得または設定します。
    /// </summary>
    public Offset Origin
    {
        get;
        set
        {
            ValidateOffset(value);
            if (field == value) return;
            field = value;
            MarkNeedsPaint();
        }
    }

    /// <summary>
    /// ボックス内で変換原点を決める配置を取得または設定します。
    /// <see langword="null"/>の場合、左上を変換原点として使用します。
    /// </summary>
    public Alignment? Alignment
    {
        get;
        set
        {
            if (value is { } alignment)
            {
                ValidateAlignment(alignment);
            }

            if (field == value) return;
            field = value;
            MarkNeedsPaint();
        }
    }

    /// <summary>
    /// ヒットテスト座標へ変換行列の逆変換を適用するかを取得または設定します。
    /// </summary>
    public bool TransformHitTests { get; set; } = true;

    /// <inheritdoc/>
    public override void Paint(PaintingContext context, Offset offset)
    {
        if (Child is null)
        {
            Layer = null;
            return;
        }

        var effectiveTransform = GetEffectiveTransform() * Matrix3x2.CreateTranslation((float)offset.X, (float)offset.Y);
        Layer = context.PushTransform(ToSkMatrix(effectiveTransform), base.Paint, Layer as TransformLayer);
    }

    /// <inheritdoc/>
    public override bool HitTestChildren(HitTestResult result, Offset position)
    {
        if (!TransformHitTests)
        {
            return base.HitTestChildren(result, position);
        }

        if (!Matrix3x2.Invert(GetEffectiveTransform(), out var inverse))
        {
            return false;
        }

        var transformed = Vector2.Transform(new Vector2((float)position.X, (float)position.Y), inverse);
        return Child?.HitTest(result, new Offset(transformed.X, transformed.Y)) ?? false;
    }

    private Matrix3x2 GetEffectiveTransform()
    {
        var pivot = Origin;
        if (Alignment is { } alignment)
        {
            var alignmentOffset = alignment.Pivot(Size);
            pivot += new Offset(Size.Width / 2f, Size.Height / 2f) + alignmentOffset;
        }

        return Matrix3x2.CreateTranslation((float)-pivot.X, (float)-pivot.Y)
               * Transform
               * Matrix3x2.CreateTranslation((float)pivot.X, (float)pivot.Y);
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
        Persp2 = 1
    };

    private static void ValidateMatrix(Matrix3x2 matrix)
    {
        if (!float.IsFinite(matrix.M11) || !float.IsFinite(matrix.M12)
            || !float.IsFinite(matrix.M21) || !float.IsFinite(matrix.M22)
            || !float.IsFinite(matrix.M31) || !float.IsFinite(matrix.M32))
        {
            throw new ArgumentOutOfRangeException(nameof(matrix), matrix, "変換行列の各成分には有限値を指定してください。");
        }
    }

    private static void ValidateOffset(Offset offset)
    {
        if (!double.IsFinite(offset.X) || !double.IsFinite(offset.Y))
        {
            throw new ArgumentOutOfRangeException(nameof(offset), offset, "変換原点には有限値を指定してください。");
        }
    }

    private static void ValidateAlignment(Alignment alignment)
    {
        if (!float.IsFinite(alignment.X) || !float.IsFinite(alignment.Y))
        {
            throw new ArgumentOutOfRangeException(nameof(alignment), alignment, "配置値には有限値を指定してください。");
        }
    }
}
