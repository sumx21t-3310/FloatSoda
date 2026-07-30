using System.Numerics;
using FloatSoda.Abstractions.Geometries;
using FloatSoda.Geometrics;
using FloatSoda.RenderObjects.Painting;

namespace FloatSoda.Widgets.Paint;

/// <summary>
/// レイアウト後の子要素へ2次元アフィン変換を適用します。
/// </summary>
/// <remarks>
/// 変換は子要素が占有するレイアウト寸法へ影響しません。
/// </remarks>
/// <seealso cref="RenderTransform"/>
public record Transform : SingleChildRenderObjectWidget<RenderTransform>
{
    /// <summary>
    /// 子要素へ適用する2次元アフィン変換を取得します。
    /// </summary>
    public Matrix3x2 Matrix
    {
        get;
        init
        {
            ValidateMatrix(value);
            field = value;
        }
    } = Matrix3x2.Identity;

    /// <summary>
    /// 変換原点へ加算するローカル座標のオフセットを取得します。
    /// </summary>
    public Offset Origin
    {
        get;
        init
        {
            if (!double.IsFinite(value.X) || !double.IsFinite(value.Y))
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "変換原点には有限値を指定してください。");
            }

            field = value;
        }
    }

    /// <summary>
    /// ボックス内で変換原点を決める配置を取得します。
    /// <see langword="null"/>の場合、左上を変換原点として使用します。
    /// </summary>
    public Alignment? Alignment
    {
        get;
        init
        {
            if (value is { } alignment && (!float.IsFinite(alignment.X) || !float.IsFinite(alignment.Y)))
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "配置値には有限値を指定してください。");
            }

            field = value;
        }
    }

    /// <summary>
    /// ヒットテスト座標へ変換行列の逆変換を適用するかを取得します。
    /// </summary>
    public bool TransformHitTests { get; init; } = true;

    /// <inheritdoc/>
    public override RenderTransform CreateRenderObject() => new()
    {
        Transform = Matrix,
        Origin = Origin,
        Alignment = Alignment,
        TransformHitTests = TransformHitTests
    };

    /// <inheritdoc/>
    public override void UpdateRenderObject(RenderTransform renderObject)
    {
        renderObject.Transform = Matrix;
        renderObject.Origin = Origin;
        renderObject.Alignment = Alignment;
        renderObject.TransformHitTests = TransformHitTests;
    }

    private static void ValidateMatrix(Matrix3x2 matrix)
    {
        if (!float.IsFinite(matrix.M11) || !float.IsFinite(matrix.M12)
            || !float.IsFinite(matrix.M21) || !float.IsFinite(matrix.M22)
            || !float.IsFinite(matrix.M31) || !float.IsFinite(matrix.M32))
        {
            throw new ArgumentOutOfRangeException(nameof(matrix), matrix, "変換行列の各成分には有限値を指定してください。");
        }
    }
}
