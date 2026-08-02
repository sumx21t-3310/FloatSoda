using System.Numerics;
using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.Painting;
using FloatSoda.Widgets.Paint;

namespace FloatSoda.Widgets.Layout;

/// <summary>
/// 装飾、寸法、配置、および変換を組み合わせる便利なウィジェットです。
/// </summary>
/// <remarks>
/// FlutterのContainerと同様に、内側から配置、装飾、寸法制約、変換の順で合成します。
/// PaddingはIssue #192の実装を取り込んだ後に追加します。
/// </remarks>
public record Container : StatelessWidget
{
    /// <summary>
    /// コンテナー内へ配置する子ウィジェットを取得します。
    /// </summary>
    public Widget? Child { get; init; }

    /// <summary>
    /// 子ウィジェットをコンテナー内へ配置する基準位置を取得します。
    /// <see langword="null"/>の場合、配置ウィジェットを追加しません。
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
    /// 背景へ描画する単色を取得します。
    /// <see langword="null"/>の場合、単色の背景を追加しません。
    /// </summary>
    /// <remarks>
    /// <see cref="Decoration"/>と同時には指定できません。
    /// </remarks>
    public Color? Color { get; init; }

    /// <summary>
    /// 背景へ描画するボックス装飾を取得します。
    /// <see langword="null"/>の場合、装飾を追加しません。
    /// </summary>
    /// <remarks>
    /// <see cref="Color"/>と同時には指定できません。
    /// </remarks>
    public BoxDecoration? Decoration { get; init; }

    /// <summary>
    /// コンテナーの幅を論理ピクセル単位で取得します。
    /// <see langword="null"/>の場合、幅を固定しません。
    /// </summary>
    public double? Width
    {
        get;
        init
        {
            ValidateDimension(value);
            field = value;
        }
    }

    /// <summary>
    /// コンテナーの高さを論理ピクセル単位で取得します。
    /// <see langword="null"/>の場合、高さを固定しません。
    /// </summary>
    public double? Height
    {
        get;
        init
        {
            ValidateDimension(value);
            field = value;
        }
    }

    /// <summary>
    /// レイアウト後のコンテナーへ適用する2次元変換を取得します。
    /// <see langword="null"/>の場合、変換ウィジェットを追加しません。
    /// </summary>
    public Matrix3x2? Transform
    {
        get;
        init
        {
            if (value is { } matrix)
            {
                ValidateMatrix(matrix);
            }

            field = value;
        }
    }

    /// <summary>
    /// <see cref="Transform"/>の変換原点をボックス内で決める配置を取得します。
    /// </summary>
    public Alignment? TransformAlignment
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

    /// <inheritdoc/>
    public override Widget Build(IBuildContext context)
    {
        if (Color is not null && Decoration is not null)
        {
            throw new InvalidOperationException(
                "ColorとDecorationは同時に指定できません。背景色はBoxDecoration.Colorへ指定してください。");
        }

        Widget current = Child ?? new SizedBox();

        if (Child is not null && Alignment is { } alignment)
        {
            current = new Align
            {
                Alignment = alignment,
                Child = current
            };
        }

        var effectiveDecoration = Decoration
                                  ?? (Color is { } color ? new BoxDecoration { Color = color } : null);
        if (effectiveDecoration is not null)
        {
            current = new DecoratedBox
            {
                Decoration = effectiveDecoration,
                Child = current
            };
        }

        if (Width is not null || Height is not null)
        {
            current = new SizedBox
            {
                Width = Width,
                Height = Height,
                Child = current
            };
        }

        // TODO(#192): Paddingがmainへ入った後、装飾の内側へPaddingを合成する。

        if (Transform is { } transform)
        {
            current = new FloatSoda.Widgets.Paint.Transform
            {
                Matrix = transform,
                Alignment = TransformAlignment,
                Child = current
            };
        }

        return current;
    }

    private static void ValidateDimension(double? value)
    {
        if (value is { } dimension && (!double.IsFinite(dimension) || dimension < 0))
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "寸法には0以上の有限値を指定してください。");
        }
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
