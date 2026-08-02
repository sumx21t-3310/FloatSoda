using FloatSoda.Abstractions.Geometries;
using FloatSoda.Geometrics;
using SkiaSharp;

namespace FloatSoda.Painting;

/// <summary>
/// ボックス装飾を子要素の前面と背面のどちらへ描画するかを表します。
/// </summary>
public enum DecorationPosition
{
    /// <summary>
    /// 子要素の背面へ描画します。
    /// </summary>
    Background,

    /// <summary>
    /// 子要素の前面へ描画します。
    /// </summary>
    Foreground
}

/// <summary>
/// ボックスの一辺を描画する色と太さを表します。
/// </summary>
public readonly record struct BorderSide
{
    /// <summary>
    /// 既定の色と1論理ピクセルの太さでボーダーを初期化します。
    /// </summary>
    public BorderSide()
    {
    }

    /// <summary>
    /// ボーダーを描画しない辺を取得します。
    /// </summary>
    public static BorderSide None => new() { Width = 0 };

    /// <summary>
    /// ボーダーの色を取得します。
    /// </summary>
    public Color Color { get; init; } = new(0, 0, 0);

    /// <summary>
    /// ボーダーの太さを論理ピクセル単位で取得します。
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// 値が負数または有限値ではありません。
    /// </exception>
    public double Width
    {
        get;
        init
        {
            if (!double.IsFinite(value) || value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "ボーダーの太さには0以上の有限値を指定してください。");
            }

            field = value;
        }
    } = 1;
}

/// <summary>
/// ボックスの四辺へ適用するボーダーを表します。
/// </summary>
public sealed record Border
{
    /// <summary>
    /// 上辺の設定を取得します。
    /// </summary>
    public BorderSide Top { get; init; } = BorderSide.None;

    /// <summary>
    /// 右辺の設定を取得します。
    /// </summary>
    public BorderSide Right { get; init; } = BorderSide.None;

    /// <summary>
    /// 下辺の設定を取得します。
    /// </summary>
    public BorderSide Bottom { get; init; } = BorderSide.None;

    /// <summary>
    /// 左辺の設定を取得します。
    /// </summary>
    public BorderSide Left { get; init; } = BorderSide.None;

    /// <summary>
    /// 四辺へ同じボーダーを適用した値を作成します。
    /// </summary>
    /// <param name="side">四辺へ適用する設定。</param>
    /// <returns>四辺が同じ設定のボーダー。</returns>
    public static Border All(BorderSide side) => new()
    {
        Top = side,
        Right = side,
        Bottom = side,
        Left = side
    };

    internal bool IsUniform => Top == Right && Top == Bottom && Top == Left;
}

/// <summary>
/// ボックスの背景色、角丸、およびボーダーを表す不変の装飾です。
/// </summary>
public sealed record BoxDecoration
{
    /// <summary>
    /// 背景色を取得します。
    /// <see langword="null"/>の場合、背景を塗りつぶしません。
    /// </summary>
    public Color? Color { get; init; }

    /// <summary>
    /// 四隅へ適用する角丸半径を取得します。
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// いずれかの半径が負数または有限値ではありません。
    /// </exception>
    public BorderRadius BorderRadius
    {
        get;
        init
        {
            ValidateRadius(value.TopLeft);
            ValidateRadius(value.TopRight);
            ValidateRadius(value.BottomRight);
            ValidateRadius(value.BottomLeft);
            field = value;
        }
    } = BorderRadius.Zero;

    /// <summary>
    /// 四辺へ適用するボーダーを取得します。
    /// <see langword="null"/>の場合、ボーダーを描画しません。
    /// </summary>
    public Border? Border { get; init; }

    internal void Paint(SKCanvas canvas, SKRect bounds)
    {
        var roundRect = BorderRadius.ToRoundRect(bounds);

        if (Color is { } color)
        {
            using var backgroundPaint = new SKPaint
            {
                Color = color,
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };
            canvas.DrawRoundRect(roundRect, backgroundPaint);
        }

        if (Border is not { } border)
        {
            return;
        }

        if (border.IsUniform)
        {
            PaintUniformBorder(canvas, bounds, border.Top);
            return;
        }

        canvas.Save();
        using var clipPath = new SKPath();
        clipPath.AddRoundRect(roundRect);
        canvas.ClipPath(clipPath, antialias: true);
        PaintSide(canvas, new SKRect(bounds.Left, bounds.Top, bounds.Right, bounds.Top + (float)border.Top.Width), border.Top);
        PaintSide(canvas, new SKRect(bounds.Right - (float)border.Right.Width, bounds.Top, bounds.Right, bounds.Bottom), border.Right);
        PaintSide(canvas, new SKRect(bounds.Left, bounds.Bottom - (float)border.Bottom.Width, bounds.Right, bounds.Bottom), border.Bottom);
        PaintSide(canvas, new SKRect(bounds.Left, bounds.Top, bounds.Left + (float)border.Left.Width, bounds.Bottom), border.Left);
        canvas.Restore();
    }

    internal bool HitTest(SKRect bounds, Offset position)
    {
        using var path = new SKPath();
        path.AddRoundRect(BorderRadius.ToRoundRect(bounds));
        return path.Contains((float)position.X, (float)position.Y);
    }

    private void PaintUniformBorder(SKCanvas canvas, SKRect bounds, BorderSide side)
    {
        if (side.Width == 0)
        {
            return;
        }

        // ボーダーがボックスの短辺以上に太い場合、中心線の内側寄せ(inset)だけを制限してもストローク幅は元の値のままとなり、
        // 装飾の外形をはみ出して周囲の兄弟まで覆ってしまう。この場合はボーダーが内側を覆い尽くすため、外形を塗りつぶす。
        var shortSide = Math.Max(0, Math.Min(bounds.Width, bounds.Height));
        if (side.Width >= shortSide)
        {
            using var fillPaint = new SKPaint
            {
                Color = side.Color,
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };
            canvas.DrawRoundRect(BorderRadius.ToRoundRect(bounds), fillPaint);
            return;
        }

        var inset = (float)side.Width / 2f;
        var borderBounds = bounds;
        borderBounds.Inflate(-inset, -inset);

        using var paint = new SKPaint
        {
            Color = side.Color,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = (float)side.Width
        };
        canvas.DrawRoundRect(BorderRadius.ToRoundRect(borderBounds), paint);
    }

    private static void PaintSide(SKCanvas canvas, SKRect bounds, BorderSide side)
    {
        if (side.Width == 0)
        {
            return;
        }

        using var paint = new SKPaint
        {
            Color = side.Color,
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };
        canvas.DrawRect(bounds, paint);
    }

    private static void ValidateRadius(Radius radius)
    {
        if (!float.IsFinite(radius.X) || radius.X < 0 || !float.IsFinite(radius.Y) || radius.Y < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(radius), radius, "角丸半径には0以上の有限値を指定してください。");
        }
    }
}
